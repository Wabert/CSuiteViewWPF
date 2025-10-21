using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using CSuiteViewWPF.Controls;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// Interface for ViewModels that provide filterable data grid functionality.
    /// Any ViewModel implementing this interface can be used with the FilteredDataGridControl.
    /// </summary>
    public interface IFilterableDataGridViewModel
    {
        /// <summary>
        /// The CollectionViewSource that provides filtering and sorting for the data
        /// </summary>
        CollectionViewSource ViewSource { get; }

        /// <summary>
        /// The source data items collection (for loading data into the grid)
        /// </summary>
        ObservableCollection<FileSystemItem> Items { get; set; }

        /// <summary>
        /// Text search string that applies across all columns
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Column definitions that define how to display and filter the data
        /// </summary>
        ObservableCollection<FilterableColumnDefinition> ColumnDefinitions { get; }

        /// <summary>
        /// Get the collection of filter items for a specific column
        /// </summary>
        /// <param name="columnKey">The unique key identifying the column</param>
        /// <returns>Collection of filter items for that column</returns>
        ObservableCollection<FilterItemViewModel> GetFiltersForColumn(string columnKey);

        /// <summary>
        /// Get the command to select all items in a column's filter
        /// </summary>
        /// <param name="columnKey">The unique key identifying the column</param>
        /// <returns>Command to select all filter items</returns>
        ICommand GetSelectAllCommand(string columnKey);

        /// <summary>
        /// Get the command to deselect all items in a column's filter
        /// </summary>
        /// <param name="columnKey">The unique key identifying the column</param>
        /// <returns>Command to deselect all filter items</returns>
        ICommand GetDeselectAllCommand(string columnKey);

        /// <summary>
        /// Refresh the view to apply current filters
        /// </summary>
        void RefreshView();

        // ===== New Simplified Filtering Methods =====

        /// <summary>
        /// Applies a column filter instantly based on selected values (new instant filtering).
        /// Empty set means show all rows (no filter active).
        /// </summary>
        /// <param name="columnKey">The column to filter</param>
        /// <param name="selectedValues">Selected values (whitelist), or empty to clear filter</param>
        void ApplyColumnFilter(string columnKey, HashSet<object> selectedValues);
        
        /// <summary>
        /// Gets distinct values for a column to populate filter UI
        /// </summary>
        /// <param name="columnKey">The column key</param>
        /// <returns>List of distinct values with display strings</returns>
        List<SimpleFilterValue> GetDistinctValuesForColumn(string columnKey);
        
        /// <summary>
        /// Gets currently active filter values for a column
        /// </summary>
        /// <param name="columnKey">The column key</param>
        /// <returns>Set of currently filtered values, or empty if no filter active</returns>
        HashSet<object> GetActiveFilterValues(string columnKey);
    }
}
