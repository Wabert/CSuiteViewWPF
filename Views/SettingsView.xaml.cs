using System.Windows;
using System.Windows.Controls;
using CSuiteViewWPF.Services;

namespace CSuiteViewWPF.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Light_Checked(object sender, RoutedEventArgs e)
        {
            ThemeService.Instance.SetTheme(AppTheme.Light);
        }

        private void Dark_Checked(object sender, RoutedEventArgs e)
        {
            ThemeService.Instance.SetTheme(AppTheme.Dark);
        }
    }
}
