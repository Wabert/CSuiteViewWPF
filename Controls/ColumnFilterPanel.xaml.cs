using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF.Controls
{
    /// <summary>
    /// Simplified filter popup with instant filtering via multi-select ListBox.
    /// No OK/Cancel buttons - filtering happens immediately on selection change.
    /// </summary>
    public partial class ColumnFilterPanel : UserControl
    {
        public event EventHandler? CloseRequested;
        public event EventHandler<FilterSelectionChangedEventArgs>? FilterChanged;

        private bool _isUpdatingSelection = false;

        public ColumnFilterPanel()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string ColumnName { get; set; } = string.Empty;
        public string ColumnKey { get; set; } = string.Empty;
        public ObservableCollection<SimpleFilterValue> FilterValues { get; } = new ObservableCollection<SimpleFilterValue>();

        /// <summary>
        /// Loads distinct values into the filter listbox.
        /// Adds [Clear] as first item, then all values in ascending order.
        /// </summary>
        public void LoadValues(IEnumerable<SimpleFilterValue> values)
        {
            FilterValues.Clear();
            
            // Add [Clear] as first item
            FilterValues.Add(new SimpleFilterValue 
            { 
                DisplayValue = "[Clear]", 
                Value = null, 
                IsClearItem = true 
            });

            // Add all other values in ascending order
            foreach (var value in values.OrderBy(v => v.DisplayValue, StringComparer.CurrentCultureIgnoreCase))
            {
                FilterValues.Add(value);
            }
        }

        /// <summary>
        /// Sets the currently selected values in the listbox (when reopening filter)
        /// </summary>
        public void SetSelectedValues(HashSet<object> selectedValues)
        {
            _isUpdatingSelection = true;
            try
            {
                FilterListBox.SelectedItems.Clear();

                if (selectedValues.Count == 0)
                {
                    // No selection = show all (don't select anything)
                    return;
                }

                foreach (var item in FilterValues.Where(v => !v.IsClearItem))
                {
                    if (item.Value != null && selectedValues.Contains(item.Value))
                    {
                        FilterListBox.SelectedItems.Add(item);
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        /// <summary>
        /// Gets the currently selected values from the listbox
        /// </summary>
        public HashSet<object> GetSelectedValues()
        {
            var selected = FilterListBox.SelectedItems
                .Cast<SimpleFilterValue>()
                .Where(v => !v.IsClearItem && v.Value != null)
                .Select(v => v.Value!)
                .ToHashSet();

            return selected;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection) return;

            // Check if [Clear] was clicked
            var clearItem = e.AddedItems.Cast<SimpleFilterValue>().FirstOrDefault(v => v.IsClearItem);
            if (clearItem != null)
            {
                _isUpdatingSelection = true;
                try
                {
                    // Deselect everything including [Clear]
                    FilterListBox.SelectedItems.Clear();
                }
                finally
                {
                    _isUpdatingSelection = false;
                }

                // Notify that filter was cleared (empty set = show all)
                FilterChanged?.Invoke(this, new FilterSelectionChangedEventArgs(ColumnKey, new HashSet<object>()));
                return;
            }

            // Normal selection change - notify immediately for instant filtering
            var selectedValues = GetSelectedValues();
            FilterChanged?.Invoke(this, new FilterSelectionChangedEventArgs(ColumnKey, selectedValues));
        }
    }

    /// <summary>
    /// Represents a single value in the filter listbox
    /// </summary>
    public class SimpleFilterValue
    {
        public string DisplayValue { get; set; } = string.Empty;
        public object? Value { get; set; }
        public bool IsClearItem { get; set; }
    }

    /// <summary>
    /// Event args for filter selection changes
    /// </summary>
    public class FilterSelectionChangedEventArgs : EventArgs
    {
        public string ColumnKey { get; }
        public HashSet<object> SelectedValues { get; }

        public FilterSelectionChangedEventArgs(string columnKey, HashSet<object> selectedValues)
        {
            ColumnKey = columnKey;
            SelectedValues = selectedValues;
        }
    }
}
