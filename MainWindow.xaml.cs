using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using CSuiteViewWPF.Models;
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

    private Popup? _dragPreviewPopup;
    private TextBlock? _dragPreviewText;

        public ObservableCollection<string> FileNames { get; } = new()
        {
            "Customers.csv",
            "Orders.csv",
            "Products.csv",
            "Employees.csv",
            "InventoryReport.pdf",
            "QuarterlySummary.docx"
        };

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            
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

        private void FileList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (sender is not ListBox listBox)
            {
                return;
            }

            if (listBox.SelectedItem is not string fileName || string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            GiveFeedback += MainWindow_GiveFeedback;
            QueryContinueDrag += MainWindow_QueryContinueDrag;

            ShowDragPreview(fileName);

            try
            {
                DragDrop.DoDragDrop(listBox, fileName, DragDropEffects.Copy);
            }
            finally
            {
                HideDragPreview();
                GiveFeedback -= MainWindow_GiveFeedback;
                QueryContinueDrag -= MainWindow_QueryContinueDrag;
            }

            e.Handled = true;
        }

        private void NavigationTree_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var targetNode = GetDropTargetNode(e);
            e.Effects = targetNode is null ? DragDropEffects.None : DragDropEffects.Copy;
            e.Handled = true;
        }

        private void NavigationTree_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)))
            {
                return;
            }

            if (e.Data.GetData(typeof(string)) is not string fileName || string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var targetNode = GetDropTargetNode(e);
            if (targetNode is null)
            {
                return;
            }

            targetNode.Children ??= new ObservableCollection<NavigationNode>();

            if (targetNode.Children.Any(child => string.Equals(child.Name, fileName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var newNode = new NavigationNode
            {
                Name = fileName,
                Type = "File",
                Children = new ObservableCollection<NavigationNode>()
            };

            targetNode.Children.Add(newNode);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (NavigationTree.ItemContainerGenerator.ContainerFromItem(targetNode) is TreeViewItem targetContainer)
                {
                    targetContainer.IsExpanded = true;
                    targetContainer.UpdateLayout();

                    if (targetContainer.ItemContainerGenerator.ContainerFromItem(newNode) is TreeViewItem newContainer)
                    {
                        newContainer.IsSelected = true;
                        newContainer.BringIntoView();
                    }
                }
            }), DispatcherPriority.Background);

            e.Handled = true;
        }

        private void MainWindow_GiveFeedback(object? sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            Mouse.OverrideCursor = Cursors.Arrow;
            UpdateDragPreviewPosition(Mouse.GetPosition(this));
            e.Handled = true;
        }

        private void MainWindow_QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            UpdateDragPreviewPosition(Mouse.GetPosition(this));
        }

        private void ShowDragPreview(string fileName)
        {
            if (_dragPreviewPopup is null || _dragPreviewText is null)
            {
                _dragPreviewText = new TextBlock
                {
                    Foreground = Brushes.DarkBlue,
                    FontWeight = FontWeights.SemiBold
                };

                var border = new Border
                {
                    Background = Brushes.Gold,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(199, 137, 0)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(10, 6, 10, 6),
                    Child = _dragPreviewText
                };

                _dragPreviewPopup = new Popup
                {
                    AllowsTransparency = true,
                    IsHitTestVisible = false,
                    Placement = PlacementMode.Absolute,
                    StaysOpen = true,
                    Child = border
                };
            }

            _dragPreviewText.Text = fileName;
            UpdateDragPreviewPosition(Mouse.GetPosition(this));

            if (_dragPreviewPopup is not null)
            {
                _dragPreviewPopup.IsOpen = true;
            }
        }

        private void HideDragPreview()
        {
            if (_dragPreviewPopup is not null)
            {
                _dragPreviewPopup.IsOpen = false;
            }

            Mouse.OverrideCursor = null;
        }

        private void UpdateDragPreviewPosition(Point relativePosition)
        {
            if (_dragPreviewPopup is null)
            {
                return;
            }

            var screenPosition = PointToScreen(relativePosition);
            _dragPreviewPopup.HorizontalOffset = screenPosition.X + 12;
            _dragPreviewPopup.VerticalOffset = screenPosition.Y + 12;
        }

        private NavigationNode? GetDropTargetNode(DragEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source)
            {
                return null;
            }

            var container = FindAncestor<TreeViewItem>(source);
            return container?.DataContext as NavigationNode;
        }

        private static T? FindAncestor<T>(DependencyObject? current)
            where T : DependencyObject
        {
            while (current is not null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
