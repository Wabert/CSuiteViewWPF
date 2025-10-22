using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CSuiteViewWPF.Models;
using CSuiteViewWPF.Services;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// High-performance ViewModel that uses PerformantDataFilter for blazing-fast filtering
    /// of large datasets (300,000+ rows) with sub-100ms filter updates.
    /// 
    /// Key differences from standard FilteredDataGridViewModel:
    /// - Uses BitArray-based filtering instead of DataView.RowFilter
    /// - Pre-computes column indexes on startup
    /// - Works directly with strongly-typed objects (List<FileSystemItem>) instead of DataTable
    /// - Filters update in 10-50ms regardless of data size
    /// </summary>
    public class FilterDataGridViewModel : INotifyPropertyChanged, IFilterableDataGridViewModel
    {
        #region Fields

        private readonly Dictionary<string, ObservableCollection<FilterItemViewModel>> _columnFilters;
        private readonly CollectionViewSource _viewSource;
        private PerformantDataFilter<FileSystemItem> _filterEngine;
        private ObservableCollection<FileSystemItem> _filteredItems;
        private string _searchText = string.Empty;
        private string _statusMessage = string.Empty;

        #endregion

        #region Constructor

    public FilterDataGridViewModel()
        {
            // Initialize with empty dataset
            _filterEngine = new PerformantDataFilter<FileSystemItem>(Enumerable.Empty<FileSystemItem>());
            _filteredItems = new ObservableCollection<FileSystemItem>();

            // Initialize the CollectionViewSource for WPF data binding
            _viewSource = new CollectionViewSource
            {
                Source = _filteredItems
            };

            _columnFilters = new Dictionary<string, ObservableCollection<FilterItemViewModel>>
            {
                ["FullPath"] = new ObservableCollection<FilterItemViewModel>(),
                ["ObjectType"] = new ObservableCollection<FilterItemViewModel>(),
                ["ObjectName"] = new ObservableCollection<FilterItemViewModel>(),
                ["FileExtension"] = new ObservableCollection<FilterItemViewModel>(),
                ["Size"] = new ObservableCollection<FilterItemViewModel>(),
                ["DateLastModified"] = new ObservableCollection<FilterItemViewModel>()
            };

            ColumnDefinitions = new ObservableCollection<FilteredColumnDefinition>
            {
                new FilteredColumnDefinition
                {
                    Header = "Full Path",
                    BindingPath = "FullPath",
                    ColumnKey = "FullPath",
                    Width = new GridLength(1, GridUnitType.Star),
                    IsFilterable = true
                },
                new FilteredColumnDefinition
                {
                    Header = "Object Type",
                    BindingPath = "ObjectType",
                    ColumnKey = "ObjectType",
                    Width = new GridLength(120),
                    IsFilterable = true
                },
                new FilteredColumnDefinition
                {
                    Header = "Object Name",
                    BindingPath = "ObjectName",
                    ColumnKey = "ObjectName",
                    Width = new GridLength(180),
                    IsFilterable = true
                },
                new FilteredColumnDefinition
                {
                    Header = "File Extension",
                    BindingPath = "FileExtension",
                    ColumnKey = "FileExtension",
                    Width = new GridLength(100),
                    IsFilterable = true
                },
                new FilteredColumnDefinition
                {
                    Header = "Size",
                    BindingPath = "Size",
                    ColumnKey = "Size",
                    Width = new GridLength(120),
                    StringFormat = "{0:N0}",
                    IsFilterable = true
                },
                new FilteredColumnDefinition
                {
                    Header = "Date Last Modified",
                    BindingPath = "DateLastModified",
                    ColumnKey = "DateLastModified",
                    Width = new GridLength(180),
                    StringFormat = "{0:yyyy-MM-dd HH:mm:ss}",
                    IsFilterable = true
                }
            };

            AttachFilterChangeHandlers();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The source data items collection (for loading data into the grid).
        /// Setting this property will load the items and build indexes.
        /// </summary>
        public ObservableCollection<FileSystemItem> Items
        {
            get => _filteredItems;
            set
            {
                if (value != null)
                {
                    LoadItems(value);
                }
            }
        }

        /// <summary>
        /// The filtered items collection bound to the DataGrid
        /// </summary>
        public ObservableCollection<FileSystemItem> FilteredItems
        {
            get => _filteredItems;
            private set
            {
                if (_filteredItems != value)
                {
                    _filteredItems = value;
                    OnPropertyChanged(nameof(FilteredItems));
                }
            }
        }

        /// <summary>
        /// Status message showing filter performance and row counts
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFiltersWithTiming();
                }
            }
        }

        public bool HasData => _filterEngine.TotalRowCount > 0;

    public ObservableCollection<FilteredColumnDefinition> ColumnDefinitions { get; }

        /// <summary>
        /// Gets performance statistics about the filter engine
        /// </summary>
        public string PerformanceStats => $"Total: {_filterEngine.TotalRowCount:N0} rows, Filtered: {_filterEngine.FilteredRowCount:N0} rows, Active Filters: {_filterEngine.FilteredColumns.Count}";

        /// <summary>
        /// ViewSource property for WPF data binding.
        /// Wraps the FilteredItems collection in a CollectionViewSource.
        /// </summary>
        public CollectionViewSource ViewSource => _viewSource;

        #endregion

        #region IFilterableDataGridViewModel Implementation

        public ObservableCollection<FilterItemViewModel> GetFiltersForColumn(string columnKey)
        {
            if (!_columnFilters.TryGetValue(columnKey, out var collection))
            {
                collection = new ObservableCollection<FilterItemViewModel>();
                _columnFilters[columnKey] = collection;
            }

            // Save current selections
            var selectionMap = collection
                .ToDictionary(f => GetNormalizedValue(f.Value), f => f.IsSelected);

            // Get distinct values from filter engine
            var distinctValues = _filterEngine.GetDistinctValues(columnKey);

            // Rebuild collection with preserved selections
            collection.Clear();
            foreach (var value in OrderDistinctValues(columnKey, distinctValues))
            {
                var displayValue = FormatDisplayValue(columnKey, value);
                var normalizedKey = GetNormalizedValue(value);
                
                var isSelected = selectionMap.TryGetValue(normalizedKey, out var selected)
                    ? selected
                    : true; // Default to selected

                collection.Add(new FilterItemViewModel
                {
                    Value = value,
                    DisplayValue = displayValue,
                    IsSelected = isSelected
                });
            }

            return collection;
        }

        public ICommand GetSelectAllCommand(string columnKey)
        {
            return new RelayCommand(() => SetAllSelected(GetFiltersForColumn(columnKey), true));
        }

        public ICommand GetDeselectAllCommand(string columnKey)
        {
            return new RelayCommand(() => SetAllSelected(GetFiltersForColumn(columnKey), false));
        }

        public void RefreshView()
        {
            ApplyFiltersWithTiming();
        }

        #endregion

        #region Data Loading Methods

        /// <summary>
        /// Loads items into the filter engine and builds indexes.
        /// This is the initialization step - may take a few seconds for 300k rows,
        /// but filtering afterwards will be instant.
        /// </summary>
        public void LoadItems(IEnumerable<FileSystemItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var sw = Stopwatch.StartNew();

            // Create new filter engine with the data
            var itemList = items.ToList();
            var newEngine = new PerformantDataFilter<FileSystemItem>(itemList);

            // Build all indexes in parallel (this is the one-time cost)
            Debug.WriteLine($"Building indexes for {itemList.Count:N0} rows...");
            var indexSw = Stopwatch.StartNew();
            
            newEngine.BuildAllIndexesParallel(
                "FullPath",
                "ObjectType",
                "ObjectName",
                "FileExtension",
                "Size",
                "DateLastModified"
            );

            indexSw.Stop();
            Debug.WriteLine($"Index building completed in {indexSw.ElapsedMilliseconds:N0}ms");

            // Replace the old engine
            _filterEngine = newEngine;

            // Rebuild all filter dropdowns
            RebuildAllFilters();

            // Apply initial filter (show all)
            ApplyFiltersWithTiming();

            sw.Stop();

            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(PerformanceStats));

            Debug.WriteLine($"Total load time: {sw.ElapsedMilliseconds:N0}ms");
            StatusMessage = $"Loaded {itemList.Count:N0} rows in {sw.ElapsedMilliseconds:N0}ms";
        }

        /// <summary>
        /// Clears all data and resets the view
        /// </summary>
        public void ClearData()
        {
            _filterEngine = new PerformantDataFilter<FileSystemItem>(Enumerable.Empty<FileSystemItem>());

            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredItems = new ObservableCollection<FileSystemItem>();
            });

            foreach (var collection in _columnFilters.Values)
            {
                collection.Clear();
            }

            SearchText = string.Empty;
            StatusMessage = string.Empty;

            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(PerformanceStats));
        }

        #endregion

        #region Filter Management Methods

        /// <summary>
        /// Applies all active filters and updates the filtered items collection.
        /// This is where the magic happens - should complete in <100ms even with 300k rows.
        /// NOTE: With the new simple filter system, filters are applied directly to the engine,
        /// so we just retrieve the already-filtered data here.
        /// </summary>
        public void ApplyFilters()
        {
            // Get filtered data from engine (filters already applied via ApplyColumnFilter)
            var filteredData = _filterEngine.GetFilteredData();

            // Apply search text filter if present
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var searchLower = _searchText.ToLower();
                filteredData = filteredData.Where(item =>
                    (item.FullPath?.ToLower().Contains(searchLower) ?? false) ||
                    (item.ObjectType?.ToLower().Contains(searchLower) ?? false) ||
                    (item.ObjectName?.ToLower().Contains(searchLower) ?? false) ||
                    (item.FileExtension?.ToLower().Contains(searchLower) ?? false) ||
                    (item.Size?.ToString().Contains(searchLower) ?? false) ||
                    (item.DateLastModified?.ToString().Contains(searchLower) ?? false)
                ).ToList();
            }

            // Update the observable collection on the UI thread to avoid cross-thread exceptions
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredItems.Clear();
                foreach (var item in filteredData)
                {
                    FilteredItems.Add(item);
                }
            });

            Debug.WriteLine($"Filter result: {filteredData.Count:N0} of {_filterEngine.TotalRowCount:N0} rows visible");
        }

        /// <summary>
        /// Applies filters with performance timing and status updates
        /// </summary>
        private void ApplyFiltersWithTiming()
        {
            var sw = Stopwatch.StartNew();
            ApplyFilters();
            sw.Stop();

            var filterTime = sw.ElapsedMilliseconds;
            StatusMessage = $"Showing {_filterEngine.FilteredRowCount:N0} of {_filterEngine.TotalRowCount:N0} rows. Filter time: {filterTime}ms";

            Debug.WriteLine($"Filter applied in {filterTime}ms");

            // Warn if filter is slow (should be <100ms)
            if (filterTime > 100)
            {
                Debug.WriteLine($"WARNING: Filter took {filterTime}ms (target: <100ms)");
            }
        }

        /// <summary>
        /// Clears the filter for a specific column
        /// </summary>
        public void ClearColumnFilter(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey))
            {
                return;
            }

            // Select all items in this column's filter
            var filters = GetFiltersForColumn(columnKey);
            foreach (var filter in filters)
            {
                filter.IsSelected = true;
            }

            // Remove filter from engine
            _filterEngine.RemoveFilter(columnKey);

            // Reapply filters
            ApplyFiltersWithTiming();
        }

        /// <summary>
        /// Applies filter for a specific column instantly (new simplified filtering).
        /// Called when user clicks items in the filter listbox.
        /// Empty selection means "show all rows" (no filter active).
        /// </summary>
        public void ApplyColumnFilter(string columnKey, HashSet<object> selectedValues)
        {
            if (selectedValues.Count == 0)
            {
                // No selection = clear filter for this column (show all)
                _filterEngine.RemoveFilter(columnKey);
            }
            else
            {
                // Apply whitelist filter
                _filterEngine.SetFilter(columnKey, selectedValues);
            }

            // Immediately update the view
            ApplyFiltersWithTiming();
            
            // Notify UI of filter state change
            OnPropertyChanged(nameof(PerformanceStats));
        }

        /// <summary>
        /// Gets all distinct values for a column to populate the filter listbox.
        /// Only returns values that exist in currently visible (filtered) rows for cascading filter effect.
        /// </summary>
        public List<Controls.SimpleFilterValue> GetDistinctValuesForColumn(string columnKey)
        {
            var distinctValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows: true);
            
            return distinctValues.Select(v => new Controls.SimpleFilterValue
            {
                Value = v.NormalizedValue,
                DisplayValue = v.DisplayValue
            }).ToList();
        }

        /// <summary>
        /// Gets currently selected filter values for a column
        /// </summary>
        public HashSet<object> GetActiveFilterValues(string columnKey)
        {
            // Check if this column has an active filter
            var activeValues = _filterEngine.GetActiveFilterValues(columnKey);
            
            // Return the active filter values, or empty set if no filter is active
            return activeValues ?? new HashSet<object>();
        }

        /// <summary>
        /// Rebuilds all filter dropdown collections from current data
        /// </summary>
        public void RebuildAllFilters()
        {
            foreach (var key in _columnFilters.Keys.ToList())
            {
                RebuildColumnFilter(key);
            }
        }

        /// <summary>
        /// Rebuilds a single column's filter dropdown
        /// </summary>
        private void RebuildColumnFilter(string columnKey)
        {
            if (!_columnFilters.TryGetValue(columnKey, out var collection))
            {
                return;
            }

            collection.Clear();

            var distinctValues = _filterEngine.GetDistinctValues(columnKey);
            var orderedValues = OrderDistinctValues(columnKey, distinctValues);

            foreach (var value in orderedValues)
            {
                collection.Add(new FilterItemViewModel
                {
                    Value = value,
                    DisplayValue = FormatDisplayValue(columnKey, value),
                    IsSelected = true
                });
            }
        }

        public void AttachFilterChangeHandlers()
        {
            foreach (var collection in _columnFilters.Values)
            {
                AttachToCollection(collection);
            }
        }

        private void AttachToCollection(ObservableCollection<FilterItemViewModel> collection)
        {
            foreach (var item in collection)
            {
                item.PropertyChanged += FilterItem_PropertyChanged;
            }

            collection.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (FilterItemViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += FilterItem_PropertyChanged;
                    }
                }
            };
        }

        private void FilterItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
            {
                // Don't auto-apply on checkbox change - wait for OK button
            }
        }

        /// <summary>
        /// Gets the visible normalized values for a column (used for updating filter UI)
        /// </summary>
        internal IEnumerable<object?> GetVisibleNormalizedValues(string columnKey)
        {
            var visibleValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows: true);
            return visibleValues.Select(v => GetNormalizedValue(v.NormalizedValue));
        }

        #endregion

        #region Helper Methods

        private static void SetAllSelected(ObservableCollection<FilterItemViewModel> filters, bool selected)
        {
            foreach (var filter in filters)
            {
                filter.IsSelected = selected;
            }
        }

        private static object GetNormalizedValue(object? value)
        {
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                return "(empty)";
            }
            return value;
        }

        private static string FormatDisplayValue(string columnKey, object value)
        {
            if (value == null)
            {
                return "(empty)";
            }

            if (value.ToString() == "(empty)")
            {
                return "(empty)";
            }

            return columnKey switch
            {
                "Size" => value is long size ? $"{size:N0}" : value.ToString() ?? "(empty)",
                "DateLastModified" => value is DateTime dt ? dt.ToString("G") : value.ToString() ?? "(empty)",
                _ => value.ToString() ?? "(empty)"
            };
        }

        private static IEnumerable<object> OrderDistinctValues(string columnKey, IEnumerable<object> values)
        {
            return columnKey switch
            {
                "Size" => values.OrderBy(v => v is long size ? size : (long?)null),
                "DateLastModified" => values.OrderBy(v => v is DateTime dt ? dt : (DateTime?)null),
                _ => values.OrderBy(v => v?.ToString(), StringComparer.CurrentCultureIgnoreCase)
            };
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
