using CSuiteViewWPF.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CSuiteViewWPF
{
    public partial class NavigationTreeControl : UserControl
    {
        public NavigationTreeControl()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            NavTree.Items.Clear();
            var nodes = NavigationSampleData.GetNodes();
            foreach (var n in nodes)
            {
                NavTree.Items.Add(BuildTreeItem(n));
            }
        }

        private TreeViewItem BuildTreeItem(NavigationNode node)
        {
            var tvi = new TreeViewItem { Header = node.Name, IsExpanded = true };
            if (node.Children != null && node.Children.Any())
            {
                foreach (var c in node.Children)
                {
                    tvi.Items.Add(BuildTreeItem(c));
                }
            }
            return tvi;
        }

        private void NavTree_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void NavTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var text = (string)e.Data.GetData(DataFormats.Text);
                var tvi = new TreeViewItem { Header = text };
                NavTree.Items.Add(tvi);
            }
        }
    }
}
