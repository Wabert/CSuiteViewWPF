using System;
using System.Windows;

namespace CSuiteViewWPF
{
    public partial class TemplateCreatorWindow : Window
    {
        public TemplateCreatorWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HeaderHeightBox.Text, out var headerH)) headerH = 48;
            if (!int.TryParse(FooterHeightBox.Text, out var footerH)) footerH = 36;
            if (!int.TryParse(PanelCountBox.Text, out var count)) count = 3;
            if (!int.TryParse(SpaceAboveBox.Text, out var above)) above = 12;
            if (!int.TryParse(SpaceBelowBox.Text, out var below)) below = 18;

            var w = new TemplateMainWindow();
            // set the header/title text from the creator input
            w.HeaderTitle = TitleBox.Text ?? string.Empty;
            w.PanelCount = count;
            w.HeaderHeight = headerH;
            w.FooterHeight = footerH;
            w.SpaceAbove = above;
            w.SpaceBelow = below;
            if (LeftTreeCheck.IsChecked == true)
            {
                w.TreePanelIndex = 0; // leftmost panel
            }
            w.Show();
        }

        private void DragTextBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tb, tb.Text, DragDropEffects.Copy);
            }
        }
    }
}
