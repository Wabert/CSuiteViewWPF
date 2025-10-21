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

            // Make window draggable by header
            if (HeaderBorder != null)
            {
                HeaderBorder.MouseLeftButtonDown += (s, e) =>
                {
                    // Only drag if not clicking on a button or other interactive element
                    if (e.OriginalSource is System.Windows.Shapes.Shape || 
                        e.OriginalSource is System.Windows.Controls.Border ||
                        e.OriginalSource is System.Windows.Controls.TextBlock)
                    {
                        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                            this.DragMove();
                    }
                };
            }

            // Make window draggable by footer
            if (ContentBorder != null)
            {
                ContentBorder.MouseLeftButtonDown += (s, e) =>
                {
                    // Only drag if clicking on the border itself, not buttons
                    if (e.OriginalSource is System.Windows.Controls.Border)
                    {
                        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                            this.DragMove();
                    }
                };
            }
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
