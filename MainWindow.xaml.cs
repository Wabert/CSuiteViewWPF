using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;

namespace CSuiteViewWPF
{
    public partial class MainWindow : MetroWindow
    {
        // Windows 11 rounded corners API
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;        // Standard rounded corners (MORE rounding)
        private const int DWMWCP_ROUNDSMALL = 3;   // Small rounded corners (LESS rounding)

        public MainWindow()
        {
            InitializeComponent();
            
            // Enable Windows 11 maximum rounded corners
            this.SourceInitialized += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                // Use ROUND for MAXIMUM corner rounding (not ROUNDSMALL)
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreWindow();
            }
            else
            {
                this.DragMove();
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking on the header or footer
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestoreWindow();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeRestoreWindow()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void SnapButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Snap functionality to be implemented", "Snap", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ScanDirButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Directory Scanner functionality to be implemented", "Scan Directory", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DbLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Database Library functionality to be implemented", "Database Library", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FormBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Form Builder functionality to be implemented", "Form Builder", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
