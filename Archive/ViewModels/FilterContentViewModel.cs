using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// ViewModel for the FilterContent control.
    /// Provides filter UI data including title, filter items, and commands.
    /// </summary>
    public class FilterContentViewModel : INotifyPropertyChanged
    {
    private string _searchText = string.Empty;
    private CollectionViewSource _filterItemsView;
    private readonly Dictionary<object, bool> _originalSelections = new();
    private static readonly object NullSelectionKey = new();
    private HashSet<object?>? _visibleNormalizedValues;

        public FilterContentViewModel()
        {
            _filterItemsView = new CollectionViewSource();
            _filterItemsView.Filter += FilterItemsView_Filter;
        }

        /// <summary>
        /// Title displayed at the top of the filter panel
        /// </summary>
        public string FilterTitle { get; set; } = string.Empty;

        /// <summary>
        /// Collection of filter items to display in the checklist
        /// </summary>
        public ObservableCollection<FilterItemViewModel> FilterItems 
        { 
            get => _filterItemsView.Source as ObservableCollection<FilterItemViewModel> ?? new();
            set
            {
                _filterItemsView.Source = value;
                
                // Store original selections
                _originalSelections.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        _originalSelections[GetSelectionKey(item.Value)] = item.IsSelected;
                    }
                }
                
                OnPropertyChanged(nameof(FilterItems));
                OnPropertyChanged(nameof(FilterItemsView));
            }
        }

        /// <summary>
        /// Normalized values that should remain visible in the UI. When null, all items are shown.
        /// </summary>
        public IEnumerable<object?>? VisibleNormalizedValues
        {
            get => _visibleNormalizedValues;
            set
            {
                _visibleNormalizedValues = value != null
                    ? new HashSet<object?>(value)
                    : null;

                OnPropertyChanged(nameof(VisibleNormalizedValues));
                _filterItemsView.View?.Refresh();
            }
        }

        /// <summary>
        /// Filtered view of filter items based on search text
        /// </summary>
        public ICollectionView FilterItemsView => _filterItemsView.View;

        /// <summary>
        /// Search text to filter the displayed items
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    _filterItemsView.View?.Refresh();
                }
            }
        }

        /// <summary>
        /// Command to select all filter items
        /// </summary>
        public ICommand? SelectAllCommand { get; set; }

        /// <summary>
        /// Command to deselect all filter items
        /// </summary>
        public ICommand? DeselectAllCommand { get; set; }

        /// <summary>
        /// Command to apply filter changes (OK button)
        /// </summary>
        public ICommand? OkCommand { get; set; }

        /// <summary>
        /// Command to cancel filter changes (Cancel button)
        /// </summary>
        public ICommand? CancelCommand { get; set; }

        /// <summary>
        /// Command to clear the filter for this column (Clear Filter button)
        /// </summary>
        public ICommand? ClearFilterCommand { get; set; }

        /// <summary>
        /// Command to sort ascending
        /// </summary>
        public ICommand? SortAscendingCommand { get; set; }

        /// <summary>
        /// Command to sort descending
        /// </summary>
        public ICommand? SortDescendingCommand { get; set; }

        /// <summary>
        /// Restores the original filter selections (called when Cancel is clicked)
        /// </summary>
        public void RestoreOriginalSelections()
        {
            foreach (var item in FilterItems)
            {
                if (_originalSelections.TryGetValue(GetSelectionKey(item.Value), out bool originalSelection))
                {
                    item.IsSelected = originalSelection;
                }
            }
        }

        private static object GetSelectionKey(object? value)
        {
            return value ?? NullSelectionKey;
        }

        private void FilterItemsView_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is FilterItemViewModel item)
            {
                if (_visibleNormalizedValues != null)
                {
                    var normalizedValue = NormalizeFilterValue(item.Value);
                    if (!_visibleNormalizedValues.Contains(normalizedValue))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = item.DisplayValue?.ToLower().Contains(SearchText.ToLower()) ?? false;
                }
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
