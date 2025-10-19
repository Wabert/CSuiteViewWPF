using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// ViewModel for FileSystemItem data with filterable DataGrid support.
    /// This is now a concrete implementation of IFilterableDataGridViewModel.
    /// </summary>
    public class FilteredDataGridViewModel : INotifyPropertyChanged, IFilterableDataGridViewModel
    {
        private ObservableCollection<FileSystemItem>? _items;
        private string _searchText = string.Empty;
    private CollectionViewSource _viewSource = new CollectionViewSource();

        public ObservableCollection<FileSystemItem>? Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
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

        // Filter options for each column - stored in dictionary for easy lookup by column key
        private Dictionary<string, ObservableCollection<FilterItemViewModel>> _columnFilters;
        
        // Properties that access the dictionary collections
        public ObservableCollection<FilterItemViewModel> FullPathFilters => _columnFilters["FullPath"];
        public ObservableCollection<FilterItemViewModel> ObjectTypeFilters => _columnFilters["ObjectType"];
        public ObservableCollection<FilterItemViewModel> ObjectNameFilters => _columnFilters["ObjectName"];
        public ObservableCollection<FilterItemViewModel> FileExtensionFilters => _columnFilters["FileExtension"];
        public ObservableCollection<FilterItemViewModel> SizeFilters => _columnFilters["Size"];
        public ObservableCollection<FilterItemViewModel> DateLastModifiedFilters => _columnFilters["DateLastModified"];

        // Column definitions for the DataGrid
        public ObservableCollection<FilterableColumnDefinition> ColumnDefinitions { get; private set; }

        public FilteredDataGridViewModel()
        {
            ViewSource = new CollectionViewSource();
            ViewSource.Filter += ViewSource_Filter;
            
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
        }

        private void UpdateViewSource()
        {
            if (Items != null)
            {
                ViewSource.Source = Items;
                ViewSource.View.Refresh();
            }
        }

        private void UpdateFilterOptions()
        {
            var columnKeys = _columnFilters.Keys.ToList();

            foreach (var key in columnKeys)
            {
                _columnFilters[key].Clear();
            }

            if (Items == null || Items.Count == 0)
            {
                return;
            }

            foreach (var key in columnKeys)
            {
                var collection = _columnFilters[key];
                foreach (var valueInfo in BuildFilterValues(key, Items))
                {
                    collection.Add(new FilterItemViewModel
                    {
                        Value = valueInfo.RawValue,
                        DisplayValue = valueInfo.DisplayValue,
                        IsSelected = true
                    });
                }
            }
        }

        private void ViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not FileSystemItem item)
            {
                e.Accepted = false;
                return;
            }

            // Check search
            if (!string.IsNullOrEmpty(SearchText))
            {
                var search = SearchText.ToLower();
                if (!(item.FullPath?.ToLower().Contains(search) == true ||
                      item.ObjectType?.ToLower().Contains(search) == true ||
                      item.ObjectName?.ToLower().Contains(search) == true ||
                      item.FileExtension?.ToLower().Contains(search) == true ||
                      item.Size?.ToString().Contains(search) == true ||
                      item.DateLastModified?.ToString().Contains(search) == true))
                {
                    e.Accepted = false;
                    return;
                }
            }

            // Check filters
            bool fullPathOk = IsSelected(FullPathFilters, item.FullPath);
            bool objectTypeOk = IsSelected(ObjectTypeFilters, item.ObjectType);
            bool objectNameOk = IsSelected(ObjectNameFilters, item.ObjectName);
            bool fileExtensionOk = IsSelected(FileExtensionFilters, item.FileExtension);
            bool sizeOk = IsSelected(SizeFilters, item.Size);
            bool dateOk = IsSelected(DateLastModifiedFilters, item.DateLastModified);
            
            // DIAGNOSTIC: Log which filter is blocking items
            if (!fullPathOk || !objectTypeOk || !objectNameOk || !fileExtensionOk || !sizeOk || !dateOk)
            {
                System.Diagnostics.Debug.WriteLine($"ITEM FILTERED OUT: {item.ObjectName}");
                System.Diagnostics.Debug.WriteLine($"  FullPath OK: {fullPathOk} (value: '{item.FullPath}')");
                System.Diagnostics.Debug.WriteLine($"  ObjectType OK: {objectTypeOk} (value: '{item.ObjectType}')");
                System.Diagnostics.Debug.WriteLine($"  ObjectName OK: {objectNameOk} (value: '{item.ObjectName}')");
                System.Diagnostics.Debug.WriteLine($"  FileExtension OK: {fileExtensionOk} (value: '{item.FileExtension}')");
                System.Diagnostics.Debug.WriteLine($"  Size OK: {sizeOk} (value: {item.Size})");
                System.Diagnostics.Debug.WriteLine($"  DateLastModified OK: {dateOk} (value: {item.DateLastModified})");
                
                e.Accepted = false;
                return;
            }

            e.Accepted = true;
        }

        private bool IsSelected(ObservableCollection<FilterItemViewModel> filters, object? value)
        {
            if (filters.Count == 0) return true;

            var normalizedKey = CreateFilterKey(value);

            var matchingFilter = filters.FirstOrDefault(f =>
                CreateFilterKey(f.Value).Equals(normalizedKey));

            if (matchingFilter == null)
            {
                return true;
            }

            return matchingFilter.IsSelected;
        }

        public void ApplyFilters()
        {
            ViewSource.View.Refresh();
        }

        // Methods to select/deselect all for each column
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
            // ApplyFilters();
        }

        // Attach event handlers to filter changes
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
                // ApplyFilters();
            }
        }

        // IFilterableDataGridViewModel interface implementation
        public ObservableCollection<FilterItemViewModel> GetFiltersForColumn(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey))
            {
                _columnFilters[columnKey] = new ObservableCollection<FilterItemViewModel>();
            }

            var collection = _columnFilters[columnKey];

            if (Items == null || Items.Count == 0)
            {
                return collection;
            }

            var selectionMap = collection
                .GroupBy(f => CreateFilterKey(f.Value))
                .ToDictionary(g => g.Key, g => g.First().IsSelected);

            var valueInfos = BuildFilterValues(columnKey, Items);

            collection.Clear();
            foreach (var info in valueInfos)
            {
                var normalizedKey = info.NormalizedKey;
                var isSelected = selectionMap.TryGetValue(normalizedKey, out var selected)
                    ? selected
                    : true;

                collection.Add(new FilterItemViewModel
                {
                    Value = info.RawValue,
                    DisplayValue = info.DisplayValue,
                    IsSelected = isSelected
                });
            }

            return collection;
        }

        /// <summary>
        /// Gets the items that are currently visible based on all applied filters
        /// </summary>
        private List<FileSystemItem> GetCurrentlyVisibleItems()
        {
            if (Items == null || ViewSource.View == null)
            {
                return new List<FileSystemItem>();
            }

            var visibleItems = new List<FileSystemItem>();
            foreach (var item in ViewSource.View)
            {
                if (item is FileSystemItem fileItem)
                {
                    visibleItems.Add(fileItem);
                }
            }
            
            return visibleItems;
        }

        private static object? NormalizeFilterValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string s && string.IsNullOrEmpty(s))
            {
                return null;
            }

            return value;
        }

        private static FilterKey CreateFilterKey(object? value)
        {
            return new FilterKey(NormalizeFilterValue(value));
        }

        private IEnumerable<FilterValueInfo> BuildFilterValues(string columnKey, IEnumerable<FileSystemItem> items)
        {
            var groupedValues = GetColumnValues(columnKey, items)
                .Select(v => new { Raw = v, Key = CreateFilterKey(v) })
                .GroupBy(x => x.Key);

            var result = new List<FilterValueInfo>();

            foreach (var group in groupedValues)
            {
                object? rawValue = group.Select(x => x.Raw).FirstOrDefault(v => v != null);
                if (rawValue == null && group.Any())
                {
                    rawValue = group.First().Raw;
                }

                var displayValue = rawValue?.ToString();
                if (string.IsNullOrEmpty(displayValue))
                {
                    displayValue = "(empty)";
                }

                result.Add(new FilterValueInfo(rawValue, group.Key, displayValue));
            }

            return OrderFilterValues(columnKey, result);
        }

        private IEnumerable<FilterValueInfo> OrderFilterValues(string columnKey, IEnumerable<FilterValueInfo> values)
        {
            return columnKey switch
            {
                "Size" => values.OrderBy(v => v.RawValue as long?),
                "DateLastModified" => values.OrderBy(v => v.RawValue as DateTime?),
                _ => values.OrderBy(v => v.DisplayValue, StringComparer.CurrentCultureIgnoreCase)
            };
        }

        private IEnumerable<object?> GetColumnValues(string columnKey, IEnumerable<FileSystemItem> items)
        {
            return columnKey switch
            {
                "FullPath" => items.Select(i => (object?)i.FullPath),
                "ObjectType" => items.Select(i => (object?)i.ObjectType),
                "ObjectName" => items.Select(i => (object?)i.ObjectName),
                "FileExtension" => items.Select(i => (object?)i.FileExtension),
                "Size" => items.Select(i => (object?)i.Size),
                "DateLastModified" => items.Select(i => (object?)i.DateLastModified),
                _ => Enumerable.Empty<object?>()
            };
        }

        internal IEnumerable<object?>? GetVisibleNormalizedValues(string columnKey)
        {
            var visibleItems = GetCurrentlyVisibleItems();
            if (visibleItems.Count == 0)
            {
                return Array.Empty<object?>();
            }

            var normalizedValues = new HashSet<object?>();

            foreach (var value in GetColumnValues(columnKey, visibleItems))
            {
                normalizedValues.Add(NormalizeFilterValue(value));
            }

            return normalizedValues;
        }

        private sealed class FilterValueInfo
        {
            public FilterValueInfo(object? rawValue, FilterKey normalizedKey, string displayValue)
            {
                RawValue = rawValue;
                NormalizedKey = normalizedKey;
                DisplayValue = displayValue;
            }

            public object? RawValue { get; }
            public FilterKey NormalizedKey { get; }
            public string DisplayValue { get; }
        }

        private readonly struct FilterKey : IEquatable<FilterKey>
        {
            private readonly object? _value;

            public FilterKey(object? value)
            {
                _value = value;
            }

            public bool Equals(FilterKey other)
            {
                return Equals(_value, other._value);
            }

            public override bool Equals(object? obj)
            {
                return obj is FilterKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _value?.GetHashCode() ?? 0;
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
        /// Clears all filters for a specific column by selecting all items and rebuilding from full dataset
        /// </summary>
        public void ClearColumnFilter(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey))
            {
                return;
            }

            // ALWAYS rebuild from the full dataset when clearing
            // This ensures we get ALL values, not just currently visible ones
            RebuildColumnFilter(columnKey);
            
            // Notify that the filter collection changed
            OnPropertyChanged($"{columnKey}Filters");
            
            // CRITICAL: Apply the filters to refresh the view!
            ApplyFilters();
            
        }

        /// <summary>
        /// Rebuilds a single column's filter collection from the full dataset
        /// </summary>
        private void RebuildColumnFilter(string columnKey)
        {
            if (!_columnFilters.ContainsKey(columnKey))
            {
                return;
            }

            var collection = _columnFilters[columnKey];

            collection.Clear();

            if (Items == null || Items.Count == 0)
            {
                return;
            }

            foreach (var valueInfo in BuildFilterValues(columnKey, Items))
            {
                collection.Add(new FilterItemViewModel
                {
                    Value = valueInfo.RawValue,
                    DisplayValue = valueInfo.DisplayValue,
                    IsSelected = true
                });
            }
        }

        /// <summary>
        /// Rebuilds all filter collections from the full dataset (used when data is first loaded)
        /// </summary>
        public void RebuildAllFilters()
        {
            if (Items == null || Items.Count == 0)
            {
                return;
            }

            foreach (var columnKey in _columnFilters.Keys.ToList())
            {
                ClearColumnFilter(columnKey);
            }

            // Refresh the view
            ViewSource?.View?.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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