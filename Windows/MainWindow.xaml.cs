using System.Windows;
using CSuiteViewWPF.Views;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Main application window inheriting from ThemedWindow for consistent chrome and theming.
    /// MVVM Pattern: Minimal code-behind - all logic is in MainWindowViewModel.
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // Title is bound in the template via {TemplateBinding Title}
            // Additional window-specific initialization can be placed here as needed.
        }
    }
}
