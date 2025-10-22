using CSuiteViewWPF.Models;
using CSuiteViewWPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CSuiteViewWPF.Controls
{
    /// <summary>
    /// UserControl providing styled window chrome (header, footer, gold frame).
    /// Can be hosted in any Window to provide consistent styling.
    /// Content should be added to the MiddleBorder area.
    /// </summary>
    public partial class StyledWindowBase : UserControl
    {
        public StyledWindowBase()
        {
            InitializeComponent();

            // If no DataContext is set externally, create default ViewModel
            if (DataContext == null)
            {
                DataContext = new StyledContentWindowViewModel();
            }

            // Sync ViewModel with UI after load
            Loaded += StyledWindowBase_Loaded;
        }

        // Public accessors for named elements
        public TextBlock HeaderTitleText => HeaderTitleTextBlock;
        public Border Footer => FooterBar;
        public Rectangle FooterSeparator => FooterLine;

        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void FooterBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void StyledWindowBase_Loaded(object sender, RoutedEventArgs e)
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
                // Note: FilteredDataGrid removed - content is now added dynamically to MiddleBorder
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
            // FilteredDataGrid removed - content is now added dynamically to MiddleBorder
            // Sample data loading no longer needed in the template window
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}