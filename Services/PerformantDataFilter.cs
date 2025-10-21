using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// High-performance filtering engine using BitArray indexing for near-instant filtering
    /// on large datasets (300k+ rows). Uses pre-computed indexes to achieve sub-100ms filter updates.
    /// </summary>
    /// <typeparam name="T">The data model type to filter</typeparam>
    public class PerformantDataFilter<T> where T : class
    {
        #region Fields

        // Core data storage
        private readonly List<T> _sourceData;
        private List<T> _filteredData;

        // Column indexes: ColumnName -> (Value -> BitArray of matching rows)
        // Each BitArray has length = _sourceData.Count, where true = row has that value
        private readonly Dictionary<string, Dictionary<object, BitArray>> _columnIndexes;

        // Cached row counts: ColumnName -> (Value -> Count of rows with that value)
        // Cached during index building to avoid re-counting every time
        private readonly Dictionary<string, Dictionary<object, int>> _valueCounts;

        // Active filters: ColumnName -> Set of selected values
        private readonly Dictionary<string, HashSet<object>> _activeFilters;

        // Master visibility BitArray: true = row is visible after all filters applied
        private BitArray _visibleRows;

        // Cached property accessors for fast reflection
        private readonly Dictionary<string, Func<T, object?>> _propertyAccessors;

        // Performance tracking
        private readonly Stopwatch _performanceTimer;

        // Lock object for thread-safe dictionary updates
        private readonly object _indexLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Total number of rows in the source dataset
        /// </summary>
        public int TotalRowCount => _sourceData.Count;

        /// <summary>
        /// Number of rows currently visible after filters applied
        /// </summary>
        public int FilteredRowCount { get; private set; }

        /// <summary>
        /// Returns true if any filters are currently active
        /// </summary>
        public bool HasActiveFilters => _activeFilters.Count > 0;

        /// <summary>
        /// Gets the list of column names that have been indexed
        /// </summary>
        public IReadOnlyCollection<string> IndexedColumns => _columnIndexes.Keys;

        /// <summary>
        /// Gets the list of column names that have active filters
        /// </summary>
        public IReadOnlyCollection<string> FilteredColumns => _activeFilters.Keys;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new PerformantDataFilter with the specified source data
        /// </summary>
        /// <param name="sourceData">The complete dataset to filter (must not be modified externally)</param>
        public PerformantDataFilter(IEnumerable<T> sourceData)
        {
            if (sourceData == null)
                throw new ArgumentNullException(nameof(sourceData));

            _sourceData = sourceData.ToList();
            _filteredData = new List<T>(_sourceData); // Initially all rows visible
            _columnIndexes = new Dictionary<string, Dictionary<object, BitArray>>();
            _valueCounts = new Dictionary<string, Dictionary<object, int>>();
            _activeFilters = new Dictionary<string, HashSet<object>>();
            _propertyAccessors = new Dictionary<string, Func<T, object?>>();
            _performanceTimer = new Stopwatch();

            // Initialize visibility: all rows visible
            _visibleRows = new BitArray(_sourceData.Count, true);
            FilteredRowCount = _sourceData.Count;
        }

        #endregion

        #region Index Building

        /// <summary>
        /// Builds an index for a single column. This creates a dictionary mapping each distinct
        /// value in the column to a BitArray indicating which rows contain that value.
        /// </summary>
        /// <param name="propertyName">The property name to index (must be a valid property on T)</param>
        public void BuildColumnIndex(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            _performanceTimer.Restart();

            // Get or create cached property accessor
            var accessor = GetOrCreatePropertyAccessor(propertyName);

            // Dictionary to hold value -> BitArray mappings
            var valueIndex = new Dictionary<object, BitArray>(new FilterValueEqualityComparer());
            var counts = new Dictionary<object, int>(new FilterValueEqualityComparer());

            // Build index by scanning all rows
            for (int rowIndex = 0; rowIndex < _sourceData.Count; rowIndex++)
            {
                var item = _sourceData[rowIndex];
                var value = accessor(item);

                // Normalize value (null/empty strings treated as single key)
                var normalizedValue = NormalizeFilterValue(value);

                // Get or create BitArray for this value
                if (!valueIndex.TryGetValue(normalizedValue, out var bitArray))
                {
                    bitArray = new BitArray(_sourceData.Count, false);
                    valueIndex[normalizedValue] = bitArray;
                    counts[normalizedValue] = 0;
                }

                // Mark this row as having this value
                bitArray[rowIndex] = true;
                counts[normalizedValue]++;
            }

            // Store the index and counts (thread-safe)
            lock (_indexLock)
            {
                _columnIndexes[propertyName] = valueIndex;
                _valueCounts[propertyName] = counts;
            }

            _performanceTimer.Stop();
            Debug.WriteLine($"[PerformantDataFilter] Built index for '{propertyName}': " +
                          $"{valueIndex.Count} distinct values, {_performanceTimer.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Builds indexes for multiple columns in parallel for maximum performance
        /// </summary>
        /// <param name="propertyNames">Array of property names to index</param>
        public void BuildAllIndexesParallel(params string[] propertyNames)
        {
            if (propertyNames == null || propertyNames.Length == 0)
                throw new ArgumentException("Must specify at least one property name", nameof(propertyNames));

            _performanceTimer.Restart();

            // Build indexes in parallel
            Parallel.ForEach(propertyNames, propertyName =>
            {
                BuildColumnIndex(propertyName);
            });

            _performanceTimer.Stop();
            Debug.WriteLine($"[PerformantDataFilter] Built {propertyNames.Length} indexes in parallel: " +
                          $"{_performanceTimer.ElapsedMilliseconds}ms total");
        }

        /// <summary>
        /// Rebuilds all existing indexes (useful if source data changes)
        /// </summary>
        public void RebuildAllIndexes()
        {
            var columnsToRebuild = _columnIndexes.Keys.ToArray();
            _columnIndexes.Clear();

            if (columnsToRebuild.Length > 0)
            {
                BuildAllIndexesParallel(columnsToRebuild);
            }
        }

        #endregion

        #region Filter Operations

        /// <summary>
        /// Sets or updates the filter for a column. Only rows with values in selectedValues will be visible.
        /// This operation is very fast (typically &lt;100ms for 300k rows) because it uses BitArray operations.
        /// </summary>
        /// <param name="propertyName">The column to filter</param>
        /// <param name="selectedValues">Set of values to show (null or empty = remove filter)</param>
        /// <returns>Time taken to apply the filter in milliseconds</returns>
        public long SetFilter(string propertyName, HashSet<object>? selectedValues)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            _performanceTimer.Restart();

            // If no values selected, remove the filter
            if (selectedValues == null || selectedValues.Count == 0)
            {
                RemoveFilter(propertyName);
                return _performanceTimer.ElapsedMilliseconds;
            }

            // Ensure column is indexed
            if (!_columnIndexes.ContainsKey(propertyName))
            {
                BuildColumnIndex(propertyName);
            }

            // Normalize selected values
            var normalizedValues = new HashSet<object>(
                selectedValues.Select(NormalizeFilterValue),
                new FilterValueEqualityComparer()
            );

            // Update active filters
            _activeFilters[propertyName] = normalizedValues;

            // Rebuild visibility BitArray
            RecalculateVisibility();

            _performanceTimer.Stop();
            Debug.WriteLine($"[PerformantDataFilter] Applied filter to '{propertyName}': " +
                          $"{selectedValues.Count} values selected, {FilteredRowCount}/{TotalRowCount} rows visible, " +
                          $"{_performanceTimer.ElapsedMilliseconds}ms");

            return _performanceTimer.ElapsedMilliseconds;
        }

        /// <summary>
        /// Removes the filter from a column, making all values visible again
        /// </summary>
        /// <param name="propertyName">The column to clear</param>
        public void RemoveFilter(string propertyName)
        {
            if (_activeFilters.Remove(propertyName))
            {
                RecalculateVisibility();
                Debug.WriteLine($"[PerformantDataFilter] Removed filter from '{propertyName}': " +
                              $"{FilteredRowCount}/{TotalRowCount} rows now visible");
            }
        }

        /// <summary>
        /// Gets the currently active filter values for a specific column
        /// </summary>
        /// <param name="propertyName">The column name</param>
        /// <returns>HashSet of active filter values, or null if no filter is active</returns>
        public HashSet<object>? GetActiveFilterValues(string propertyName)
        {
            lock (_indexLock)
            {
                return _activeFilters.TryGetValue(propertyName, out var values) ? values : null;
            }
        }

        /// <summary>
        /// Removes all filters, making all rows visible
        /// </summary>
        public void ClearAllFilters()
        {
            if (_activeFilters.Count > 0)
            {
                _activeFilters.Clear();
                _visibleRows.SetAll(true);
                FilteredRowCount = TotalRowCount;
                _filteredData = new List<T>(_sourceData);

                Debug.WriteLine($"[PerformantDataFilter] Cleared all filters: {TotalRowCount} rows visible");
            }
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Gets the filtered dataset as a list. This is a new list instance containing only visible rows.
        /// Use this to bind to the DataGrid.
        /// </summary>
        /// <returns>List containing only rows that pass all active filters</returns>
        public List<T> GetFilteredData()
        {
            return new List<T>(_filteredData);
        }

        /// <summary>
        /// Gets the filtered dataset as a read-only collection (more efficient, no copy)
        /// </summary>
        public IReadOnlyList<T> GetFilteredDataReadOnly()
        {
            return _filteredData.AsReadOnly();
        }

        /// <summary>
        /// Gets all distinct values for a column, based on CURRENTLY VISIBLE rows only.
        /// This is important for Excel-like behavior where filter dropdowns only show
        /// values that exist in the filtered dataset.
        /// </summary>
        /// <param name="propertyName">The column to get values from</param>
        /// <param name="onlyVisibleRows">If true, only returns values from currently visible rows (default: true)</param>
        /// <returns>Sorted list of distinct values with their display strings</returns>
        public List<FilterValueInfo> GetDistinctValues(string propertyName, bool onlyVisibleRows = true)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            // Ensure column is indexed
            if (!_columnIndexes.ContainsKey(propertyName))
            {
                BuildColumnIndex(propertyName);
            }

            var columnIndex = _columnIndexes[propertyName];
            var valueCounts = _valueCounts[propertyName];
            var distinctValues = new List<FilterValueInfo>();

            // Optimization: If all rows are visible (no filters active), skip the intersection check
            bool allRowsVisible = !HasActiveFilters || FilteredRowCount == TotalRowCount;

            foreach (var kvp in columnIndex)
            {
                var normalizedValue = kvp.Key;
                var rowBitArray = kvp.Value;

                // If onlyVisibleRows and not all rows are visible, check if this value exists in any visible row
                if (onlyVisibleRows && !allRowsVisible)
                {
                    // Use BitArray.And to check intersection with visible rows
                    var intersection = new BitArray(rowBitArray);
                    intersection.And(_visibleRows);

                    // If no intersection, skip this value
                    if (!HasAnyTrueBits(intersection))
                        continue;
                }

                // Use cached count instead of re-counting
                int count = valueCounts[normalizedValue];

                distinctValues.Add(new FilterValueInfo
                {
                    NormalizedValue = normalizedValue,
                    DisplayValue = FormatFilterValue(normalizedValue),
                    RowCount = count
                });
            }

            // Sort values (nulls first, then alphabetically)
            distinctValues.Sort((a, b) =>
            {
                if (a.NormalizedValue is NullFilterValue && b.NormalizedValue is not NullFilterValue)
                    return -1;
                if (a.NormalizedValue is not NullFilterValue && b.NormalizedValue is NullFilterValue)
                    return 1;
                return string.Compare(a.DisplayValue, b.DisplayValue, StringComparison.OrdinalIgnoreCase);
            });

            return distinctValues;
        }

        /// <summary>
        /// Gets the currently active filter values for a column
        /// </summary>
        /// <param name="propertyName">The column name</param>
        /// <returns>Set of selected values, or null if no filter active</returns>
        public HashSet<object>? GetActiveFilter(string propertyName)
        {
            return _activeFilters.TryGetValue(propertyName, out var values) ? values : null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Recalculates the master visibility BitArray by combining all active filters.
        /// This is the core performance optimization: uses BitArray.And() operations which
        /// are extremely fast (bitwise AND operations on 64-bit words).
        /// </summary>
        private void RecalculateVisibility()
        {
            _performanceTimer.Restart();

            // Start with all rows visible
            _visibleRows.SetAll(true);

            // Apply each active filter using BitArray AND operations
            foreach (var filter in _activeFilters)
            {
                var propertyName = filter.Key;
                var selectedValues = filter.Value;
                var columnIndex = _columnIndexes[propertyName];

                // Create a BitArray for this filter (OR of all selected values)
                var filterBitArray = new BitArray(_sourceData.Count, false);

                foreach (var selectedValue in selectedValues)
                {
                    if (columnIndex.TryGetValue(selectedValue, out var valueBitArray))
                    {
                        // OR: row is visible if it has ANY of the selected values
                        filterBitArray.Or(valueBitArray);
                    }
                }

                // AND: row must satisfy ALL filters (across different columns)
                _visibleRows.And(filterBitArray);
            }

            // Rebuild the filtered data list
            _filteredData.Clear();
            _filteredData.Capacity = Math.Max(_filteredData.Capacity, CountTrueBits(_visibleRows));

            for (int i = 0; i < _visibleRows.Count; i++)
            {
                if (_visibleRows[i])
                {
                    _filteredData.Add(_sourceData[i]);
                }
            }

            FilteredRowCount = _filteredData.Count;

            _performanceTimer.Stop();
            Debug.WriteLine($"[PerformantDataFilter] Recalculated visibility: " +
                          $"{FilteredRowCount}/{TotalRowCount} rows visible, " +
                          $"{_performanceTimer.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Gets or creates a compiled property accessor for fast value extraction.
        /// Thread-safe for use in parallel index building.
        /// </summary>
        private Func<T, object?> GetOrCreatePropertyAccessor(string propertyName)
        {
            // First check without lock (fast path)
            if (_propertyAccessors.TryGetValue(propertyName, out var accessor))
                return accessor;

            // Need to create accessor - use lock to prevent concurrent dictionary updates
            lock (_indexLock)
            {
                // Double-check pattern: another thread might have created it while we waited for lock
                if (_propertyAccessors.TryGetValue(propertyName, out accessor))
                    return accessor;

                // Use compiled expressions for fast property access (10-100x faster than PropertyInfo.GetValue)
                var parameter = Expression.Parameter(typeof(T), "item");
                var property = Expression.Property(parameter, propertyName);
                var convert = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<T, object?>>(convert, parameter);
                accessor = lambda.Compile();

                _propertyAccessors[propertyName] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Normalizes a filter value for consistent comparison.
        /// Null, empty strings, and whitespace are all treated as a single "null" value.
        /// </summary>
        private static object NormalizeFilterValue(object? value)
        {
            if (value == null)
                return NullFilterValue.Instance;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return NullFilterValue.Instance;

            return value;
        }

        /// <summary>
        /// Formats a normalized filter value for display in the UI
        /// </summary>
        private static string FormatFilterValue(object normalizedValue)
        {
            if (normalizedValue is NullFilterValue)
                return "(empty)";

            return normalizedValue.ToString() ?? "(empty)";
        }

        /// <summary>
        /// Counts the number of true bits in a BitArray (number of visible rows)
        /// </summary>
        private static int CountTrueBits(BitArray bitArray)
        {
            int count = 0;
            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Checks if a BitArray has any true bits (more efficient than counting)
        /// </summary>
        private static bool HasAnyTrueBits(BitArray bitArray)
        {
            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                    return true;
            }
            return false;
        }

        #endregion

        #region Sorting Support

        /// <summary>
        /// Sorts the filtered data by a column (ascending or descending).
        /// Note: Sorting happens on the FULL dataset before filtering, so filter behavior is consistent.
        /// </summary>
        /// <param name="propertyName">Column to sort by</param>
        /// <param name="ascending">True for ascending, false for descending</param>
        public void SortBy(string propertyName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            _performanceTimer.Restart();

            var accessor = GetOrCreatePropertyAccessor(propertyName);

            // Sort the SOURCE data (so filters remain consistent)
            if (ascending)
            {
                _sourceData.Sort((a, b) =>
                {
                    var aVal = accessor(a);
                    var bVal = accessor(b);
                    return CompareValues(aVal, bVal);
                });
            }
            else
            {
                _sourceData.Sort((a, b) =>
                {
                    var aVal = accessor(a);
                    var bVal = accessor(b);
                    return CompareValues(bVal, aVal); // Reversed for descending
                });
            }

            // Rebuild indexes (row positions have changed)
            RebuildAllIndexes();

            // Reapply filters to get correctly sorted filtered data
            if (_activeFilters.Count > 0)
            {
                RecalculateVisibility();
            }
            else
            {
                _filteredData = new List<T>(_sourceData);
            }

            _performanceTimer.Stop();
            Debug.WriteLine($"[PerformantDataFilter] Sorted by '{propertyName}' ({(ascending ? "asc" : "desc")}): " +
                          $"{_performanceTimer.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Compares two values for sorting (handles nulls, strings, IComparable)
        /// </summary>
        private static int CompareValues(object? a, object? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            if (a is IComparable comparableA && b is IComparable)
            {
                return comparableA.CompareTo(b);
            }

            return string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Represents information about a distinct filter value
    /// </summary>
    public class FilterValueInfo
    {
        /// <summary>
        /// The normalized value used for filtering (NullFilterValue for nulls/empty)
        /// </summary>
        public object NormalizedValue { get; set; } = null!;

        /// <summary>
        /// The display string for the UI (e.g., "(empty)" for nulls)
        /// </summary>
        public string DisplayValue { get; set; } = string.Empty;

        /// <summary>
        /// Number of rows (in the full dataset) that have this value
        /// </summary>
        public int RowCount { get; set; }
    }

    /// <summary>
    /// Singleton representing null/empty filter values.
    /// Using a dedicated type instead of null allows it to be a dictionary key.
    /// </summary>
    public sealed class NullFilterValue
    {
        public static readonly NullFilterValue Instance = new NullFilterValue();
        private NullFilterValue() { }
        public override string ToString() => "(empty)";
        public override int GetHashCode() => 0;
        public override bool Equals(object? obj) => obj is NullFilterValue;
    }

    /// <summary>
    /// Custom equality comparer for filter values that handles NullFilterValue correctly
    /// </summary>
    public class FilterValueEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is NullFilterValue && y is NullFilterValue) return true;
            if (x is NullFilterValue || y is NullFilterValue) return false;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            if (obj is NullFilterValue) return 0;
            return obj?.GetHashCode() ?? 0;
        }
    }

    #endregion
}
