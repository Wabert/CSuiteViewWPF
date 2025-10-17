using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CSuiteViewWPF.Converters
{
    public class BrushToSelectionBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is SolidColorBrush sb)
                {
                    // create a semi-transparent version for selection
                    var c = sb.Color;
                    return new SolidColorBrush(Color.FromArgb(0x22, c.R, c.G, c.B));
                }
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xD7, 0x00));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
