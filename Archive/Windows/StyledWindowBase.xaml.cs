using CSuiteViewWPF.Models;
using CSuiteViewWPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Archived legacy StyledWindowBase (UserControl). Use Views.StyleWindow instead.
    /// </summary>
    public partial class StyledWindowBase : UserControl
    {
        public StyledWindowBase()
        {
            InitializeComponent();

            if (DataContext == null)
            {
                DataContext = new StyledContentWindowViewModel();
            }

            Loaded += StyledWindowBase_Loaded;
        }

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
                if (HeaderTitleTextBlock != null)
                {
                    HeaderTitleTextBlock.Text = vm.HeaderTitle;
                }

                if (FooterBar != null)
                {
                    FooterBar.Visibility = vm.FooterVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
