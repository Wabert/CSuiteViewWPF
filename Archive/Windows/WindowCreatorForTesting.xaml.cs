using System.Windows;
using System.Windows.Controls;
using CSuiteViewWPF.Controls;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Archived: Window creator for testing. Superseded by Views.StyleWindow.
    /// </summary>
    public partial class WindowCreatorForTesting : UserControl
    {
        public WindowCreatorForTesting()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = TitleBox.Text ?? "Test Window",
                Width = 800,
                Height = 600,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(6)
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(window, chrome);

            var styledControl = new StyledWindowBase();
            styledControl.HeaderTitleText.Text = TitleBox.Text ?? "Test Window";

            window.Content = styledControl;
            
            window.Show();
        }
    }
}
