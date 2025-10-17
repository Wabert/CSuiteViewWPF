using CSuiteViewWPF.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSuiteViewWPF
{
    public partial class TreeViewControl : UserControl
    {
        public TreeViewControl()
        {
            InitializeComponent();
            // Prevent TreeView from auto-scrolling selected items into view
            NavTree.AddHandler(RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(NavTree_RequestBringIntoView), true);
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            NavTree.Items.Clear();
            var nodes = TreeViewControlSampleData.GetNodes();
            foreach (var n in nodes)
            {
                NavTree.Items.Add(BuildTreeItem(n));
            }
        }

        private TreeViewItem BuildTreeItem(Models.TreeViewControlNode node)
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

        private void NavTree_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // If the request comes from a TreeViewItem (or its visual child), suppress it so clicking
            // an item doesn't automatically scroll it to the top of the ScrollViewer.
            DependencyObject source = e.OriginalSource as DependencyObject;
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            if (source is TreeViewItem)
            {
                e.Handled = true;
            }
        }

        // Handler wired via EventSetter in XAML for individual TreeViewItems
        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Prevent the built-in TreeViewItem behavior that scrolls the item into view
            e.Handled = true;
        }
    }
}
