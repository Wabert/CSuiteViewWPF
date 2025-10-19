using System.ComponentModel;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// Generic filter item that can be used for any filterable column.
    /// Represents a single value in a filter list with selection state.
    /// </summary>
    public class FilterItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = true;

        /// <summary>
        /// The actual value being filtered (could be string, int, DateTime, etc.)
        /// </summary>
    public object? Value { get; set; }

        /// <summary>
        /// Display string for the UI (formatted version of Value)
        /// </summary>
        public string DisplayValue { get; set; } = string.Empty;

        /// <summary>
        /// Whether this filter item is currently selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
