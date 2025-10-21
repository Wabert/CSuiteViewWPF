using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Window creator for testing different window configurations
    /// </summary>
    public partial class WindowCreatorForTesting : UserControl
    {
        public WindowCreatorForTesting()
        {
            InitializeComponent();
            // TableDisplayControl removed - now using FilteredDataGridControl
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HeaderHeightBox.Text, out var headerH)) headerH = 48;
            if (!int.TryParse(FooterHeightBox.Text, out var footerH)) footerH = 36;
            if (!int.TryParse(PanelCountBox.Text, out var count)) count = 3;
            if (!int.TryParse(SpaceAboveBox.Text, out var above)) above = 12;
            if (!int.TryParse(SpaceBelowBox.Text, out var below)) below = 18;

            var w = new StyledContentWindow();
            w.HeaderTitle = TitleBox.Text ?? string.Empty;
            w.PanelCount = count;
            w.HeaderHeight = headerH;
            w.FooterHeight = footerH;
            w.SpaceAbove = above;
            w.SpaceBelow = below;
            w.FooterVisible = (FooterVisibleCheck.IsChecked == true);
            if (LeftTreeCheck.IsChecked == true)
            {
                w.TreePanelIndex = 0; // leftmost panel
            }
            w.Show();
        }

        
    }
}
