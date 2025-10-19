using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSuiteViewWPF.Services;
using CSuiteViewWPF.Windows;

namespace CSuiteViewWPF.ViewModels
{
    /// <summary>
    /// ViewModel for the main startup window with three action buttons
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly FileService _fileService = new FileService();

        public MainWindowViewModel()
        {
            // Initialize commands
            ScreenshotCommand = new RelayCommand(ExecuteScreenshot);
            DirectoryScanCommand = new RelayCommand(ExecuteDirectoryScan);
            TestWindowsCommand = new RelayCommand(ExecuteTestWindows);
        }

        #region Commands

        public ICommand ScreenshotCommand { get; }
        public ICommand DirectoryScanCommand { get; }
        public ICommand TestWindowsCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteScreenshot()
        {
            // TODO: Implement screenshot functionality
            System.Windows.MessageBox.Show("Screenshot functionality coming soon!", "Screenshot");
        }

        private void ExecuteDirectoryScan()
        {
            var window = new StyledContentWindow
            {
                Width = 1000,
                Height = 600,
                DataContext = new StyledContentWindowViewModel
                {
                    HeaderTitle = "File System Scanner",
                    PanelCount = 0,
                    FooterVisible = true  // Show footer for status messages
                }
            };
            
            // Replace the FilteredDataGrid with our custom content after window loads
            window.Loaded += (s, e) =>
            {
                try
                {
                    // Find the MiddleBorder that contains the FilteredDataGrid
                    var middleBorder = FindVisualChildByName<Border>(window, "MiddleBorder");
                    if (middleBorder != null)
                    {
                        // Create our custom content
                        var content = new FileSystemScannerWindow();
                        
                        // Replace the child of MiddleBorder
                        middleBorder.Child = content;
                    }
                    else
                    {
                        MessageBox.Show("Could not find MiddleBorder in StyledContentWindow. The window layout may have changed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        window.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Directory Scanner:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.Close();
                }
            };
            
            window.Show();
        }

        private static T? FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null)
                return default(T);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && typedChild.Name == name)
                    return typedChild;

                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return default(T);
        }

        private void ExecuteTestWindows()
        {
            var window = new StyledContentWindow
            {
                DataContext = new StyledContentWindowViewModel
                {
                    HeaderTitle = "Window Creator - Testing",
                    PanelCount = 0,
                    FooterVisible = false
                }
            };

            // Set the WindowCreatorForTesting as the middle content
            var creator = new WindowCreatorForTesting();
            var middleBorder = window.FindName("MiddleBorder") as System.Windows.Controls.Border;
            if (middleBorder != null)
            {
                middleBorder.Child = creator;
                middleBorder.Visibility = System.Windows.Visibility.Visible;
            }

            window.Show();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
