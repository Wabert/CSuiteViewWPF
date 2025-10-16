using System;
using System.Globalization;
using System.Windows.Data;

namespace CSuiteViewWPF.Converters
{
    // Placeholder converter to satisfy generated code references.
    // The application no longer uses this converter; this keeps builds stable.
    public class HeaderToButtonSizeConverter : IValueConverter
    {
        // Properties to match previous XAML usage (keeps old resource attributes valid)
        public double MaxSize { get; set; } = 30.0;
        public double Padding { get; set; } = 8.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // return a sensible default if ever used accidentally
            if (value is double d)
            {
                return Math.Min(MaxSize, Math.Max(0.0, d - Padding));
            }
            return 30.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
// removed: converter not needed after reverting close-button sizing
// file left intentionally blank to allow safe deletion
