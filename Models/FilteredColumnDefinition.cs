using System.Windows;

namespace CSuiteViewWPF.Models
{
    /// <summary>
    /// Defines metadata for a filterable column in a DataGrid.
    /// This allows the DataGrid to be configured dynamically without hard-coding column definitions.
    /// </summary>
    public class FilteredColumnDefinition
    {
        /// <summary>
        /// Display text for the column header
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Property path to bind to (e.g., "FullPath", "ObjectType", "Size")
        /// </summary>
        public string BindingPath { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier for this column (used for filter lookups)
        /// </summary>
        public string ColumnKey { get; set; } = string.Empty;

        /// <summary>
        /// Column width (e.g., "*", "120", "Auto")
        /// </summary>
        public GridLength Width { get; set; } = new GridLength(1, GridUnitType.Star);

        /// <summary>
        /// String format for displaying values (e.g., "{0:N0}" for numbers, "{0:yyyy-MM-dd}" for dates)
        /// </summary>
        public string StringFormat { get; set; } = string.Empty;

        /// <summary>
        /// Whether this column should have a filter popup
        /// </summary>
        public bool IsFilterable { get; set; } = true;

        /// <summary>
        /// Type of filter to use for this column
        /// </summary>
        public FilterType FilterType { get; set; } = FilterType.CheckList;
    }

    /// <summary>
    /// Types of filters that can be applied to columns
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Checkbox list of unique values (default for most columns)
        /// </summary>
        CheckList,

        /// <summary>
        /// Text search/contains filter
        /// </summary>
        TextSearch,

        /// <summary>
        /// Numeric range filter (min/max)
        /// </summary>
        NumericRange,

        /// <summary>
        /// Date range filter (start/end dates)
        /// </summary>
        DateRange
    }
}
