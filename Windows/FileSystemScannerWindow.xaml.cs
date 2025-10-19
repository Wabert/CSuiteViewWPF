using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CSuiteViewWPF.Models;
using CSuiteViewWPF.ViewModels;

namespace CSuiteViewWPF.Windows
{
    public partial class FileSystemScannerWindow : UserControl
    {
        private FilteredDataGridViewModel _viewModel = null!;
        private StyledContentWindow? _parentWindow;
        private CancellationTokenSource? _cancellationTokenSource;

        public FileSystemScannerWindow()
        {
            InitializeComponent();
            
            // Initialize after the control is loaded
            this.Loaded += FileSystemScannerWindow_Loaded;
        }

        private void FileSystemScannerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeDataGrid();
            
            // Try to get the active File Explorer path
            string explorerPath = GetActiveFileExplorerPath();
            if (!string.IsNullOrEmpty(explorerPath) && Directory.Exists(explorerPath))
            {
                FolderPathTextBox.Text = explorerPath;
            }
            else
            {
                FolderPathTextBox.Text = @"C:\";
            }
            
            // Find and store reference to parent window
            _parentWindow = Window.GetWindow(this) as StyledContentWindow;
            if (_parentWindow != null)
            {
                // Make sure footer is visible and set initial status
                _parentWindow.FooterVisible = true;
                SetStatusText("Ready");
            }
        }

        private void InitializeDataGrid()
        {
            // The FilteredDataGridControl creates its own ViewModel internally
            // We just need to get a reference to it
            if (FileSystemDataGrid != null && FileSystemDataGrid.ViewModel != null)
            {
                _viewModel = FileSystemDataGrid.ViewModel;
            }
        }

        private string GetActiveFileExplorerPath()
        {
            try
            {
                // Simple approach: Try to get clipboard text if it's a valid path
                // User can copy path from File Explorer address bar before opening scanner
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    if (Directory.Exists(clipboardText))
                    {
                        return clipboardText;
                    }
                }
            }
            catch
            {
                // If clipboard access fails, just return empty
            }
            return string.Empty;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement folder browser dialog
            // For now, show a message
            MessageBox.Show("Folder browser will be implemented. For now, please type or paste the folder path.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            SetStatusText("Scan cancelled by user.");
        }

        private void SetStatusText(string text)
        {
            if (_parentWindow != null)
            {
                // Find the footer TextBlock and update its text
                var footerBar = FindVisualChildByName<Border>(_parentWindow, "FooterBar");
                if (footerBar != null)
                {
                    var textBlock = FindVisualChild<TextBlock>(footerBar);
                    if (textBlock != null)
                    {
                        textBlock.Text = text;
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return default(T);

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return default(T);
        }

        private static T? FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null)
                return default(T);

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && typedChild.Name == name)
                    return typedChild;

                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return default(T);
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FolderPathTextBox.Text.Trim();

            if (string.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show("Please enter a folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("The specified folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();

            // Disable the scan button and enable stop button
            ScanButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SetStatusText("Scanning...");

            try
            {
                var items = await Task.Run(() => ScanDirectory(folderPath, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
                
                _viewModel.Items = new ObservableCollection<FileSystemItem>(items);
                _viewModel.RebuildAllFilters();
                
                // Count visible items after filters are applied (exclude NewItemPlaceholder)
                int visibleCount = 0;
                if (_viewModel.ViewSource.View != null)
                {
                    visibleCount = _viewModel.ViewSource.View.OfType<FileSystemItem>().Count();
                }
                
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    SetStatusText($"Scan stopped. {visibleCount} out of {items.Count} shown.");
                }
                else
                {
                    SetStatusText($"Scan complete. {visibleCount} out of {items.Count} shown.");
                }
            }
            catch (OperationCanceledException)
            {
                SetStatusText("Scan cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatusText("Scan failed.");
            }
            finally
            {
                ScanButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private ObservableCollection<FileSystemItem> ScanDirectory(string rootPath, CancellationToken cancellationToken)
        {
            var items = new ObservableCollection<FileSystemItem>();

            try
            {
                // Don't add the root directory itself, just scan its contents
                
                // Scan directories
                ScanDirectoriesRecursive(rootPath, items, cancellationToken);
                
                // Scan files in root
                if (!cancellationToken.IsCancellationRequested)
                {
                    ScanFilesInDirectory(rootPath, items, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Just return what we have so far
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }
            catch (Exception)
            {
                // Skip other errors
            }

            return items;
        }

        private void ScanDirectoriesRecursive(string path, ObservableCollection<FileSystemItem> items, CancellationToken cancellationToken)
        {
            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(path);
                
                foreach (var dir in directories)
                {
                    // Check for cancellation before processing each directory
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        
                        // Test if we can actually access this directory by checking for subdirectories
                        // This will throw UnauthorizedAccessException if we don't have access
                        var testAccess = dirInfo.GetFileSystemInfos();
                        
                        items.Add(new FileSystemItem
                        {
                            FullPath = dirInfo.FullName,
                            ObjectType = "Folder",
                            ObjectName = dirInfo.Name,
                            FileExtension = "",
                            Size = null,
                            DateLastModified = dirInfo.LastWriteTime
                        });

                        // Scan files in this directory
                        ScanFilesInDirectory(dir, items, cancellationToken);

                        // Recursively scan subdirectories
                        ScanDirectoriesRecursive(dir, items, cancellationToken);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip directories we don't have access to - don't add them to the list
                    }
                    catch (Exception)
                    {
                        // Skip other errors - don't add them to the list
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }
            catch (Exception)
            {
                // Skip other errors
            }
        }

        private void ScanFilesInDirectory(string path, ObservableCollection<FileSystemItem> items, CancellationToken cancellationToken)
        {
            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var files = Directory.GetFiles(path);
                
                foreach (var file in files)
                {
                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        var fileInfo = new FileInfo(file);
                        
                        items.Add(new FileSystemItem
                        {
                            FullPath = fileInfo.FullName,
                            ObjectType = "File",
                            ObjectName = fileInfo.Name,
                            FileExtension = fileInfo.Extension,
                            Size = fileInfo.Length,
                            DateLastModified = fileInfo.LastWriteTime
                        });
                    }
                    catch (Exception)
                    {
                        // Skip files we can't access
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }
            catch (Exception)
            {
                // Skip other errors
            }
        }
    }
}
