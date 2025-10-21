using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// High-performance ViewModel for FileSystemItem data with filterable DataGrid support.
    /// Uses BitArray-based filtering engine for sub-100ms filter updates on 300k+ row datasets.
    /// This is a concrete implementation of IFilterableDataGridViewModel.
    /// </summary>
    public class FilteredDataGridViewModel : INotifyPropertyChanged, IFilterableDataGridViewModel
    {
        #region Fields

        private ObservableCollection<FileSystemItem>? _items;
        private string _searchText = string.Empty;
        private CollectionViewSource _viewSource = new CollectionViewSource();

        // High-performance filtering engine
        private PerformantDataFilter<FileSystemItem>? _filterEngine;
        private FilterPerformanceMonitor _performanceMonitor;

        // Filter options for each column - stored in dictionary for easy lookup by column key
        private Dictionary<string, ObservableCollection<FilterItemViewModel>> _columnFilters;

        // Cached filter selections for each column
        private Dictionary<string, HashSet<object>> _activeFilterSelections;

        // Filtered data collection for DataGrid binding (updated in-place for performance)
        private ObservableCollection<FileSystemItem> _filteredCollection;

        // Cache flags to track which columns need cache refresh
        private HashSet<string> _cacheNeedsRefresh;

        // Flag to indicate cache is being updated (prevent recursive updates)
        private bool _isUpdatingCache;

        #endregion

        #region Properties

        /// <summary>
        /// Source data collection. When set, builds indexes in parallel for high-performance filtering.
        /// </summary>
        public ObservableCollection<FileSystemItem>? Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
                InitializeFilterEngine();
                UpdateViewSource();
                UpdateFilterOptions();
            }
        }

        public CollectionViewSource ViewSource
        {
            get => _viewSource;
            private set
            {
                _viewSource = value;
                OnPropertyChanged(nameof(ViewSource));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilters();
            }
        }

        // Filter collection properties for each column
        public ObservableCollection<FilterItemViewModel> FullPathFilters => _columnFilters["FullPath"];
        public ObservableCollection<FilterItemViewModel> ObjectTypeFilters => _columnFilters["ObjectType"];
        public ObservableCollection<FilterItemViewModel> ObjectNameFilters => _columnFilters["ObjectName"];
        public ObservableCollection<FilterItemViewModel> FileExtensionFilters => _columnFilters["FileExtension"];
        public ObservableCollection<FilterItemViewModel> SizeFilters => _columnFilters["Size"];
        public ObservableCollection<FilterItemViewModel> DateLastModifiedFilters => _columnFilters["DateLastModified"];

        /// <summary>
        /// Column definitions for the DataGrid
        /// </summary>
        public ObservableCollection<FilterableColumnDefinition> ColumnDefinitions { get; private set; }

        /// <summary>
        /// Row count display: "Showing X of Y rows"
        /// </summary>
        public string RowCountDisplay
        {
            get
            {
                if (_filterEngine == null)
                    return "No data";

                if (_filterEngine.FilteredRowCount == _filterEngine.TotalRowCount)
                    return $"Showing all {_filterEngine.TotalRowCount:N0} rows";

                return $"Showing {_filterEngine.FilteredRowCount:N0} of {_filterEngine.TotalRowCount:N0} rows";
            }
        }

        #endregion

        #region Constructor

        public FilteredDataGridViewModel()
        {
            try
            {
                // Initialize performance logging
                FilterPerformanceLogger.Initialize();
                FilterPerformanceLogger.Log("Constructor", 0, "Starting FilteredDataGridViewModel construction");

                _filteredCollection = new ObservableCollection<FileSystemItem>();
                FilterPerformanceLogger.Log("Constructor", 0, "Created _filteredCollection");

                ViewSource = new CollectionViewSource();
                ViewSource.Source = _filteredCollection; // Initialize with empty collection so View is not null
                FilterPerformanceLogger.Log("Constructor", 0, "Initialized ViewSource");

                _performanceMonitor = new FilterPerformanceMonitor();
                _activeFilterSelections = new Dictionary<string, HashSet<object>>();
                _cacheNeedsRefresh = new HashSet<string>();
                _isUpdatingCache = false;
                FilterPerformanceLogger.Log("Constructor", 0, "Initialized internal fields");

                // Initialize the filter collections dictionary
                _columnFilters = new Dictionary<string, ObservableCollection<FilterItemViewModel>>
                {
                    ["FullPath"] = new ObservableCollection<FilterItemViewModel>(),
                    ["ObjectType"] = new ObservableCollection<FilterItemViewModel>(),
                    ["ObjectName"] = new ObservableCollection<FilterItemViewModel>(),
                    ["FileExtension"] = new ObservableCollection<FilterItemViewModel>(),
                    ["Size"] = new ObservableCollection<FilterItemViewModel>(),
                    ["DateLastModified"] = new ObservableCollection<FilterItemViewModel>()
                };
                FilterPerformanceLogger.Log("Constructor", 0, "Initialized _columnFilters");
            }
            catch (Exception ex)
            {
                FilterPerformanceLogger.Log("Constructor_ERROR", 0, $"EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            try
            {
                // Define the columns for FileSystemItem data
                ColumnDefinitions = new ObservableCollection<FilterableColumnDefinition>
                {
                    new FilterableColumnDefinition
                    {
                        Header = "Full Path",
                        BindingPath = "FullPath",
                        ColumnKey = "FullPath",
                        Width = new GridLength(1, GridUnitType.Star),
                        IsFilterable = true
                    },
                    new FilterableColumnDefinition
                    {
                        Header = "Object Type",
                        BindingPath = "ObjectType",
                        ColumnKey = "ObjectType",
                        Width = new GridLength(120),
                        IsFilterable = true
                    },
                    new FilterableColumnDefinition
                    {
                        Header = "Object Name",
                        BindingPath = "ObjectName",
                        ColumnKey = "ObjectName",
                        Width = new GridLength(180),
                        IsFilterable = true
                    },
                    new FilterableColumnDefinition
                    {
                        Header = "File Extension",
                        BindingPath = "FileExtension",
                        ColumnKey = "FileExtension",
                        Width = new GridLength(100),
                        IsFilterable = true
                    },
                    new FilterableColumnDefinition
                    {
                        Header = "Size",
                        BindingPath = "Size",
                        ColumnKey = "Size",
                        Width = new GridLength(120),
                        StringFormat = "{0:N0}",
                        IsFilterable = true
                    },
                    new FilterableColumnDefinition
                    {
                        Header = "Date Last Modified",
                        BindingPath = "DateLastModified",
                        ColumnKey = "DateLastModified",
                        Width = new GridLength(180),
                        StringFormat = "{0:yyyy-MM-dd HH:mm:ss}",
                        IsFilterable = true
                    }
                };
                FilterPerformanceLogger.Log("Constructor", 0, "Initialized ColumnDefinitions");
            }
            catch (Exception ex)
            {
                FilterPerformanceLogger.Log("Constructor_ColumnDef_ERROR", 0, $"EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            FilterPerformanceLogger.Log("Constructor", 0, "FilteredDataGridViewModel construction complete");
        }

        #endregion

        #region Filter Engine Initialization

        /// <summary>
        /// Initializes the high-performance filter engine and builds all column indexes and caches in parallel
        /// </summary>
        private void InitializeFilterEngine()
        {
            if (Items == null || Items.Count == 0)
            {
                _filterEngine = null;
                return;
            }

            using (var timer = new FilterPerformanceLogger.Timer("InitializeFilterEngine", $"{Items.Count:N0} rows", 1000))
            {
                // Create filter engine with source data
                _filterEngine = new PerformantDataFilter<FileSystemItem>(Items);

                // Build all column indexes in parallel for maximum performance
                var columnsToBuild = ColumnDefinitions
                    .Where(c => c.IsFilterable)
                    .Select(c => c.BindingPath)
                    .ToArray();

                _filterEngine.BuildAllIndexesParallel(columnsToBuild);

                FilterPerformanceLogger.LogWithRowCount("BuildAllIndexes", timer.ElapsedMilliseconds, Items.Count,
                    $"{columnsToBuild.Length} columns");

                // Pre-populate all filter caches (aggressive caching for instant dropdown opening)
                BuildAllFilterCaches();

                _performanceMonitor.RecordMetric("InitializeFilterEngine", timer.ElapsedMilliseconds, Items.Count,
                    $"{columnsToBuild.Length} columns");
            }
        }

        /// <summary>
        /// Builds filter caches for ALL columns in parallel.
        /// This is the key optimization: cache all distinct values upfront so dropdown opening is instant.
        /// </summary>
        private void BuildAllFilterCaches()
        {
            if (_filterEngine == null) return;

            using (var timer = new FilterPerformanceLogger.Timer("BuildAllFilterCaches", "", 500))
            {
                var columnKeys = _columnFilters.Keys.ToArray();

                // Build all caches sequentially (ObservableCollections are not thread-safe)
                foreach (var columnKey in columnKeys)
                {
                    BuildSingleFilterCache(columnKey, onlyVisibleRows: false);
                }

                FilterPerformanceLogger.Log("BuildAllFilterCaches", timer.ElapsedMilliseconds,
                    $"{columnKeys.Length} columns cached");
            }
        }

        /// <summary>
        /// Builds the filter cache for a single column.
        /// Gets distinct values from the filter engine and populates the FilterItemViewModel collection.
        /// </summary>
        private void BuildSingleFilterCache(string columnKey, bool onlyVisibleRows)
        {
            if (_filterEngine == null) return;

            var sw = Stopwatch.StartNew();

            var collection = _columnFilters[columnKey];

            // Save current selection state
            var selectionMap = new Dictionary<object, bool>(new FilterValueEqualityComparer());
            foreach (var item in collection)
            {
                if (item.Value != null)
                    selectionMap[item.Value] = item.IsSelected;
            }

            // Get distinct values from the high-performance engine
            var distinctValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows);

            // Clear and rebuild the collection
            collection.Clear();
            foreach (var valueInfo in distinctValues)
            {
                var isSelected = selectionMap.TryGetValue(valueInfo.NormalizedValue, out var selected)
                    ? selected
                    : true;

                collection.Add(new FilterItemViewModel
                {
                    Value = valueInfo.NormalizedValue,
                    DisplayValue = valueInfo.DisplayValue,
                    IsSelected = isSelected
                });
            }

            sw.Stop();
            FilterPerformanceLogger.Log($"BuildCache_{columnKey}", sw.ElapsedMilliseconds,
                $"{distinctValues.Count} distinct values, onlyVisible={onlyVisibleRows}");
        }

        /// <summary>
        /// Updates filter caches for visible values after a filter is applied.
        /// This runs asynchronously so it doesn't block the UI.
        /// </summary>
        private async void UpdateFilterCachesAsync()
        {
            if (_filterEngine == null || _isUpdatingCache) return;

            _isUpdatingCache = true;

            try
            {
                using (var timer = new FilterPerformanceLogger.Timer("UpdateFilterCachesAsync", "", 300))
                {
                    var columnKeys = _columnFilters.Keys.ToArray();

                    // Calculate distinct values in background
                    var cacheData = await System.Threading.Tasks.Task.Run(() =>
                    {
                        var result = new Dictionary<string, List<FilterValueInfo>>();
                        foreach (var columnKey in columnKeys)
                        {
                            var distinctValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows: true);
                            result[columnKey] = distinctValues;
                        }
                        return result;
                    });

                    // Update UI on UI thread
                    foreach (var kvp in cacheData)
                    {
                        UpdateSingleCacheWithData(kvp.Key, kvp.Value);
                    }

                    FilterPerformanceLogger.Log("UpdateFilterCachesAsync", timer.ElapsedMilliseconds,
                        $"{columnKeys.Length} columns updated");
                }
            }
            finally
            {
                _isUpdatingCache = false;
            }
        }

        /// <summary>
        /// Updates a single cache collection with pre-calculated data.
        /// Must be called on the UI thread.
        /// </summary>
        private void UpdateSingleCacheWithData(string columnKey, List<FilterValueInfo> distinctValues)
        {
            var collection = _columnFilters[columnKey];

            // Save current selection state
            var selectionMap = new Dictionary<object, bool>(new FilterValueEqualityComparer());
            foreach (var item in collection)
            {
                if (item.Value != null)
                    selectionMap[item.Value] = item.IsSelected;
            }

            // Clear and rebuild the collection
            collection.Clear();
            foreach (var valueInfo in distinctValues)
            {
                var isSelected = selectionMap.TryGetValue(valueInfo.NormalizedValue, out var selected)
                    ? selected
                    : true;

                collection.Add(new FilterItemViewModel
                {
                    Value = valueInfo.NormalizedValue,
                    DisplayValue = valueInfo.DisplayValue,
                    IsSelected = isSelected
                });
            }
        }

        #endregion

        #region View Update

        /// <summary>
        /// Updates the ViewSource with filtered data from the engine.
        /// Optimized to minimize WPF rebinding overhead.
        /// </summary>
        private void UpdateViewSource()
        {
            if (_filterEngine == null)
            {
                ViewSource.Source = null;
                OnPropertyChanged(nameof(RowCountDisplay));
                return;
            }

            using (var timer = new FilterPerformanceLogger.Timer("UpdateViewSource", "", 200))
            {
                // Get filtered data from the high-performance engine
                var filteredData = _filterEngine.GetFilteredData();

                // Apply search text filter if needed
                if (!string.IsNullOrEmpty(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredData = filteredData.Where(item =>
                        item.FullPath?.ToLower().Contains(search) == true ||
                        item.ObjectType?.ToLower().Contains(search) == true ||
                        item.ObjectName?.ToLower().Contains(search) == true ||
                        item.FileExtension?.ToLower().Contains(search) == true ||
                        item.Size?.ToString().Contains(search) == true ||
                        item.DateLastModified?.ToString().Contains(search) == true
                    ).ToList();
                }

                // Set the source directly - WPF's virtualization handles the rest
                // Use DeferRefresh to batch updates
                using (ViewSource.DeferRefresh())
                {
                    ViewSource.Source = filteredData;
                }

                FilterPerformanceLogger.LogWithRowCount("UpdateViewSource", timer.ElapsedMilliseconds, filteredData.Count);
                _performanceMonitor.RecordMetric("UpdateViewSource", timer.ElapsedMilliseconds, filteredData.Count);
                OnPropertyChanged(nameof(RowCountDisplay));
            }
        }

        #endregion

        #region Filter Options Management

        /// <summary>
        /// Updates all filter options from the source data.
        /// NOTE: This is now handled by BuildAllFilterCaches() in InitializeFilterEngine().
        /// This method is kept for backwards compatibility but does nothing.
        /// </summary>
        private void UpdateFilterOptions()
        {
            // No-op: Filter options are now pre-cached by BuildAllFilterCaches()
            // and updated asynchronously by UpdateFilterCachesAsync()
        }

        #endregion

        #region Filter Application

        /// <summary>
        /// Applies all active filters using the high-performance BitArray engine.
        /// Optimized with dirty tracking and async cache updates.
        /// </summary>
        public void ApplyFilters()
        {
            if (_filterEngine == null)
                return;

            FilterPerformanceLogger.LogSection("ApplyFilters");

            using (var timer = new FilterPerformanceLogger.Timer("ApplyFilters_Total", "", 300))
            {
                // Update active filter selections for each column
                foreach (var columnKey in _columnFilters.Keys)
                {
                    UpdateColumnFilter(columnKey);
                }

                // Update the view (this shows results to user immediately)
                UpdateViewSource();

                FilterPerformanceLogger.LogWithRowCount("ApplyFilters_Total", timer.ElapsedMilliseconds,
                    _filterEngine.FilteredRowCount, $"{_filterEngine.FilteredColumns.Count} active filters");

                _performanceMonitor.RecordMetric("ApplyFilters", timer.ElapsedMilliseconds,
                    _filterEngine.FilteredRowCount, $"{_filterEngine.FilteredColumns.Count} active filters");
            }

            // Update filter caches in background (doesn't block UI)
            // This ensures next dropdown click is instant
            UpdateFilterCachesAsync();
        }

        /// <summary>
        /// Updates the filter for a single column based on selected FilterItemViewModels
        /// </summary>
        private void UpdateColumnFilter(string columnKey)
        {
            if (_filterEngine == null)
                return;

            var filterItems = _columnFilters[columnKey];

            // Get selected values (filter out nulls for the HashSet)
            var selectedValues = new HashSet<object>();
            foreach (var item in filterItems.Where(f => f.IsSelected))
            {
                if (item.Value != null)
                {
                    selectedValues.Add(item.Value);
                }
            }

            // Check if all items are selected (no effective filter)
            bool allSelected = selectedValues.Count == filterItems.Count;

            if (allSelected)
            {
                // Remove filter for this column
                _filterEngine.RemoveFilter(columnKey);
                _activeFilterSelections.Remove(columnKey);
            }
            else
            {
                // Apply filter with selected values
                _filterEngine.SetFilter(columnKey, selectedValues);
                _activeFilterSelections[columnKey] = selectedValues;
            }
        }

        #endregion

        #region Select/Deselect All Methods

        public void SelectAllFullPath() => SetAllSelected(FullPathFilters, true);
        public void DeselectAllFullPath() => SetAllSelected(FullPathFilters, false);
        public void SelectAllObjectType() => SetAllSelected(ObjectTypeFilters, true);
        public void DeselectAllObjectType() => SetAllSelected(ObjectTypeFilters, false);
        public void SelectAllObjectName() => SetAllSelected(ObjectNameFilters, true);
        public void DeselectAllObjectName() => SetAllSelected(ObjectNameFilters, false);
        public void SelectAllFileExtension() => SetAllSelected(FileExtensionFilters, true);
        public void DeselectAllFileExtension() => SetAllSelected(FileExtensionFilters, false);
        public void SelectAllSize() => SetAllSelected(SizeFilters, true);
        public void DeselectAllSize() => SetAllSelected(SizeFilters, false);
        public void SelectAllDateLastModified() => SetAllSelected(DateLastModifiedFilters, true);
        public void DeselectAllDateLastModified() => SetAllSelected(DateLastModifiedFilters, false);

        private void SetAllSelected(ObservableCollection<FilterItemViewModel> filters, bool selected)
        {
            foreach (var filter in filters)
            {
                filter.IsSelected = selected;
            }
            // Don't call ApplyFilters() here - wait for OK button
        }

        #endregion

        #region Filter Change Handlers

        /// <summary>
        /// Attach event handlers to filter changes (called from control)
        /// </summary>
        public void AttachFilterChangeHandlers()
        {
            // Attach to all filter collections in the dictionary
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
                // Don't apply filters automatically - wait for user to click OK button
            }
        }

        #endregion

        #region IFilterableDataGridViewModel Interface

        /// <summary>
        /// Gets the filter collection for a column.
        /// Returns cached values instantly - no recalculation needed!
        /// This is the key to instant dropdown opening.
        /// </summary>
        public ObservableCollection<FilterItemViewModel> GetFiltersForColumn(string columnKey)
        {
            using (var timer = new FilterPerformanceLogger.Timer($"GetFiltersForColumn_{columnKey}", "", 50))
            {
                if (!_columnFilters.ContainsKey(columnKey))
                {
                    _columnFilters[columnKey] = new ObservableCollection<FilterItemViewModel>();
                }

                var collection = _columnFilters[columnKey];

                if (_filterEngine == null || Items == null || Items.Count == 0)
                {
                    return collection;
                }

                // Return cached collection - already populated by BuildAllFilterCaches() or UpdateFilterCachesAsync()
                // This is INSTANT - no BitArray operations, no GetDistinctValues() calls!
                FilterPerformanceLogger.Log($"GetFiltersForColumn_{columnKey}", timer.ElapsedMilliseconds,
                    $"{collection.Count} items (cached)");

                return collection;
            }
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
            ViewSource?.View?.Refresh();
        }

        /// <summary>
        /// Gets the normalized values for currently visible items in a column.
        /// Used to restrict filter dropdown to only show values that exist in filtered data.
        /// </summary>
        internal IEnumerable<object?>? GetVisibleNormalizedValues(string columnKey)
        {
            if (_filterEngine == null)
                return Array.Empty<object?>();

            // Get distinct values for visible rows only
            var visibleValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows: true);
            return visibleValues.Select(v => v.NormalizedValue).ToArray();
        }

        #endregion

        #region Filter Clearing

        /// <summary>
        /// Clears all filters for a specific column by selecting all items
        /// </summary>
        public void ClearColumnFilter(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey))
            {
                return;
            }

            if (_filterEngine == null)
                return;

            var sw = Stopwatch.StartNew();

            // Remove filter from engine
            _filterEngine.RemoveFilter(columnKey);

            // Rebuild filter collection from full dataset with all items selected
            RebuildColumnFilter(columnKey);

            // Notify that the filter collection changed
            OnPropertyChanged($"{columnKey}Filters");

            // Apply the filters to refresh the view
            UpdateViewSource();

            sw.Stop();
            Debug.WriteLine($"[FilteredDataGridViewModel] Cleared filter for '{columnKey}' in {sw.ElapsedMilliseconds}ms");

            _performanceMonitor.RecordMetric("ClearColumnFilter", sw.ElapsedMilliseconds,
                _filterEngine.FilteredRowCount, columnKey);
        }

        /// <summary>
        /// Rebuilds a single column's filter collection from the full dataset
        /// </summary>
        private void RebuildColumnFilter(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey) || _filterEngine == null)
            {
                return;
            }

            var collection = _columnFilters[columnKey];
            collection.Clear();

            // Get all distinct values (not just visible ones)
            var distinctValues = _filterEngine.GetDistinctValues(columnKey, onlyVisibleRows: false);

            foreach (var valueInfo in distinctValues)
            {
                collection.Add(new FilterItemViewModel
                {
                    Value = valueInfo.NormalizedValue,
                    DisplayValue = valueInfo.DisplayValue,
                    IsSelected = true
                });
            }
        }

        /// <summary>
        /// Rebuilds all filter collections from the full dataset
        /// </summary>
        public void RebuildAllFilters()
        {
            if (_filterEngine == null || Items == null || Items.Count == 0)
            {
                return;
            }

            foreach (var columnKey in _columnFilters.Keys.ToList())
            {
                ClearColumnFilter(columnKey);
            }

            ViewSource?.View?.Refresh();
        }

        #endregion

        #region Sorting Support

        /// <summary>
        /// Sorts the data by a column (ascending or descending)
        /// </summary>
        public void SortByColumn(string columnKey, bool ascending = true)
        {
            if (_filterEngine == null)
                return;

            using (var timer = new FilterPerformanceLogger.Timer($"SortByColumn_{columnKey}",
                ascending ? "ascending" : "descending", 1000))
            {
                _filterEngine.SortBy(columnKey, ascending);

                // Update view to show sorted data
                UpdateViewSource();

                FilterPerformanceLogger.LogWithRowCount($"SortByColumn_{columnKey}", timer.ElapsedMilliseconds,
                    _filterEngine.FilteredRowCount, ascending ? "asc" : "desc");

                _performanceMonitor.RecordMetric("SortByColumn", timer.ElapsedMilliseconds,
                    _filterEngine.FilteredRowCount, $"{columnKey} {(ascending ? "asc" : "desc")}");
            }

            // Sorting changes row order, so rebuild caches
            UpdateFilterCachesAsync();
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// Gets the performance monitor for diagnostics
        /// </summary>
        public FilterPerformanceMonitor PerformanceMonitor => _performanceMonitor;

        /// <summary>
        /// Generates a performance report for debugging
        /// </summary>
        public string GetPerformanceReport()
        {
            return _performanceMonitor.GenerateReport();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region New Simplified Filtering Methods (Not Implemented - Use PerformantFilteredDataGridViewModel)

        /// <summary>
        /// Not implemented in this ViewModel. Use PerformantFilteredDataGridViewModel for instant filtering.
        /// </summary>
        public void ApplyColumnFilter(string columnKey, HashSet<object> selectedValues)
        {
            throw new NotImplementedException("Use PerformantFilteredDataGridViewModel for instant filtering");
        }

        /// <summary>
        /// Not implemented in this ViewModel. Use PerformantFilteredDataGridViewModel for instant filtering.
        /// </summary>
        public List<Controls.SimpleFilterValue> GetDistinctValuesForColumn(string columnKey)
        {
            throw new NotImplementedException("Use PerformantFilteredDataGridViewModel for instant filtering");
        }

        /// <summary>
        /// Not implemented in this ViewModel. Use PerformantFilteredDataGridViewModel for instant filtering.
        /// </summary>
        public HashSet<object> GetActiveFilterValues(string columnKey)
        {
            throw new NotImplementedException("Use PerformantFilteredDataGridViewModel for instant filtering");
        }

        #endregion
    }

    /// <summary>
    /// Simple implementation of ICommand for use in ViewModels
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
