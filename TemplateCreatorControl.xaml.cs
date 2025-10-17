using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF
{
    public partial class TemplateCreatorControl : UserControl
    {
        public TemplateCreatorControl()
        {
            InitializeComponent();
            // populate preview table with sample data so the creator shows a live example
            var sample = new System.Collections.Generic.List<Models.TableRow>
            {
                new Models.TableRow{Phs=1,Form="EXECUL",Plancode="1U143900",IssueDate="1/25/2023",Amount="100,000",IssAge=45,Gender="M",Class="N"},
                new Models.TableRow{Phs=2,Form="CTR",Plancode="1U535300",IssueDate="1/25/2023",Amount="25,000",IssAge=45,Gender="M",Class="N"},
                new Models.TableRow{Phs=3,Form="STR",Plancode="1U535A00",IssueDate="1/25/2023",Amount="75,000",IssAge=42,Gender="F",Class="P"},
                new Models.TableRow{Phs=4,Form="EXECUL",Plancode="1U143900",IssueDate="3/25/2025",Amount="50,000",IssAge=47,Gender="M",Class="P"},
            };

            PreviewTable.DisplayData(sample, autoGenerateColumns: false);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HeaderHeightBox.Text, out var headerH)) headerH = 48;
            if (!int.TryParse(FooterHeightBox.Text, out var footerH)) footerH = 36;
            if (!int.TryParse(PanelCountBox.Text, out var count)) count = 3;
            if (!int.TryParse(SpaceAboveBox.Text, out var above)) above = 12;
            if (!int.TryParse(SpaceBelowBox.Text, out var below)) below = 18;

            var w = new TemplateMainWindow();
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
