using System.Windows;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// MVVM Pattern: Minimal code-behind - all logic is in MainWindowViewModel
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext is set in XAML to MainWindowViewModel
        }

        /// <summary>
        /// Close button handler - one of the few acceptable code-behind methods
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
