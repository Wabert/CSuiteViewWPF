using CSuiteViewWPF.Models;
using CSuiteViewWPF.ViewModels;
using System;
using System.Windows;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Styled content window with custom chrome and configurable layout.
    /// MVVM Pattern: Minimal code-behind - most logic is in StyledContentWindowViewModel
    /// </summary>
    public partial class StyledContentWindow : Window
    {
        public StyledContentWindow()
        {
            InitializeComponent();

            // If no DataContext is set externally, create default ViewModel
            if (DataContext == null)
            {
                DataContext = new StyledContentWindowViewModel();
            }

            // Allow dragging by mouse down on the header area
            if (HeaderBar != null)
            {
                HeaderBar.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                        this.DragMove();
                };
            }

            // Allow dragging by footer bar as well
            if (FooterBar != null)
            {
                FooterBar.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                        this.DragMove();
                };
            }

            // Sync ViewModel with UI after load
            Loaded += StyledContentWindow_Loaded;
        }

        private void StyledContentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as StyledContentWindowViewModel;
            if (vm != null)
            {
                // Sync header title
                if (HeaderTitleTextBlock != null)
                {
                    HeaderTitleTextBlock.Text = vm.HeaderTitle;
                }

                // Apply footer visibility
                if (FooterBar != null)
                {
                    FooterBar.Visibility = vm.FooterVisible ? Visibility.Visible : Visibility.Collapsed;
                }

                // Load sample data if Items is empty
                if (vm.Items == null || vm.Items.Count == 0)
                {
                    LoadSampleData();
                }
                else
                {
                    // Set items to FilteredDataGrid
                    if (FilteredDataGrid != null && FilteredDataGrid.ViewModel != null)
                    {
                        FilteredDataGrid.ViewModel.Items = vm.Items;
                    }
                }
            }
        }

        #region Backward Compatibility Properties

        public string HeaderTitle
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.HeaderTitle ?? string.Empty;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.HeaderTitle = value;
                }
                if (HeaderTitleTextBlock != null)
                {
                    HeaderTitleTextBlock.Text = value ?? string.Empty;
                }
            }
        }

        public int PanelCount
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.PanelCount ?? 3;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.PanelCount = value;
                }
            }
        }

        public int TreePanelIndex
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.TreePanelIndex ?? -1;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.TreePanelIndex = value;
                }
            }
        }

        public bool FooterVisible
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.FooterVisible ?? true;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.FooterVisible = value;
                }
                if (FooterBar != null)
                {
                    FooterBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public int HeaderHeight
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.HeaderHeight ?? 48;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.HeaderHeight = value;
                }
            }
        }

        public int FooterHeight
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.FooterHeight ?? 36;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.FooterHeight = value;
                }
            }
        }

        public int SpaceAbove
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.SpaceAbove ?? 12;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.SpaceAbove = value;
                }
            }
        }

        public int SpaceBelow
        {
            get
            {
                var vm = DataContext as StyledContentWindowViewModel;
                return vm?.SpaceBelow ?? 18;
            }
            set
            {
                var vm = DataContext as StyledContentWindowViewModel;
                if (vm != null)
                {
                    vm.SpaceBelow = value;
                }
            }
        }

        #endregion

        private void LoadSampleData()
        {
            if (FilteredDataGrid != null && FilteredDataGrid.ViewModel != null && FilteredDataGrid.ViewModel.Items != null && FilteredDataGrid.ViewModel.Items.Count > 0)
                return;

            var items = new System.Collections.ObjectModel.ObservableCollection<FileSystemItem>
            {
                new FileSystemItem { FullPath = "C:\\Windows\\System32\\kernel32.dll", ObjectType = "File", ObjectName = "kernel32.dll", FileExtension = ".dll", Size = 123456, DateLastModified = DateTime.Now.AddDays(-10) },
                new FileSystemItem { FullPath = "C:\\Windows\\System32", ObjectType = "Folder", ObjectName = "System32", FileExtension = "", Size = null, DateLastModified = DateTime.Now.AddDays(-5) },
                new FileSystemItem { FullPath = "C:\\Users\\user\\Desktop\\shortcut.lnk", ObjectType = ".lnk", ObjectName = "shortcut", FileExtension = ".lnk", Size = 1024, DateLastModified = DateTime.Now.AddHours(-2) },
                new FileSystemItem { FullPath = "C:\\Program Files\\app.exe", ObjectType = "File", ObjectName = "app.exe", FileExtension = ".exe", Size = 2048000, DateLastModified = DateTime.Now.AddDays(-1) },
                new FileSystemItem { FullPath = "C:\\Temp", ObjectType = "Folder", ObjectName = "Temp", FileExtension = "", Size = null, DateLastModified = DateTime.Now.AddMinutes(-30) },
            };

            if (FilteredDataGrid != null && FilteredDataGrid.ViewModel != null)
            {
                FilteredDataGrid.ViewModel.Items = items;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}