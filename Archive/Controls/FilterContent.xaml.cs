using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CSuiteViewWPF.ViewModels;

namespace CSuiteViewWPF.Controls
{
    /// <summary>
    /// Reusable filter content control that displays a multi-select list of filter options.
    /// Uses ListBox selection instead of checkboxes for better performance with large datasets.
    /// </summary>
    public partial class FilterContent : UserControl
    {
        private bool _isUpdatingSelection = false;

        public FilterContent()
        {
            InitializeComponent();
            this.Loaded += FilterContent_Loaded;
        }

        private void FilterContent_Loaded(object sender, RoutedEventArgs e)
        {
            // Sync initial selection from FilterItemViewModel.IsSelected to ListBox
            SyncListBoxSelectionFromViewModel();

            // Listen for selection changes in the ListBox
            FilterListBox.SelectionChanged += FilterListBox_SelectionChanged;

            // Listen for DataContext changes
            this.DataContextChanged += FilterContent_DataContextChanged;
        }

        private void FilterContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // When DataContext changes, re-sync selection
            if (e.NewValue is FilterContentViewModel)
            {
                SyncListBoxSelectionFromViewModel();
            }
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;
            try
            {
                // Update IsSelected based on ListBox selection
                foreach (var item in e.RemovedItems.OfType<FilterItemViewModel>())
                {
                    item.IsSelected = false;
                }

                foreach (var item in e.AddedItems.OfType<FilterItemViewModel>())
                {
                    item.IsSelected = true;
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        private void SyncListBoxSelectionFromViewModel()
        {
            if (DataContext is not FilterContentViewModel vm) return;
            if (vm.FilterItems == null) return;

            _isUpdatingSelection = true;
            try
            {
                FilterListBox.SelectedItems.Clear();
                foreach (var item in vm.FilterItems.Where(f => f.IsSelected))
                {
                    FilterListBox.SelectedItems.Add(item);
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
    }
}
