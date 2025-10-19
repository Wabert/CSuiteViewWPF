using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
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
    }
}
