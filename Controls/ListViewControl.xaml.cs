using CSuiteViewWPF.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSuiteViewWPF
{
    public partial class ListViewControl : UserControl
    {
        public ListViewControl()
        {
            InitializeComponent();
            // populate with sample data for now
            var sample = new List<Models.ListViewItem>
            {
                new Models.ListViewItem("First Name","John"),
                new Models.ListViewItem("Last Name","Doe"),
                new Models.ListViewItem("Street","1234 Main St"),
                new Models.ListViewItem("City","Springfield"),
                new Models.ListViewItem("State","IL"),
                new Models.ListViewItem("DOB","1985-03-12"),
                new Models.ListViewItem("Issue Date","2020-01-01"),
                new Models.ListViewItem("SSN","***-**-1234"),
                new Models.ListViewItem("Email","john.doe@example.com"),
                new Models.ListViewItem("Phone","(555) 123-4567"),
            };
            ItemsHost.ItemsSource = sample;

            // attach event to allow copying text on click
            ItemsHost.MouseLeftButtonUp += ItemsHost_MouseLeftButtonUp;
            ItemsHost.PreviewMouseRightButtonUp += ItemsHost_PreviewMouseRightButtonUp;
    }

    // Keep an old-style constructor name compatibility method in case some code used the old name
    // (this does not create a new type, it's purely a static convenience and not necessary)

        private void ItemsHost_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // show copy context menu for the Value under mouse
            var el = e.OriginalSource as FrameworkElement;
            var kv = el?.DataContext as Models.ListViewItem;
            var menu = new ContextMenu();

            var copyValue = new MenuItem { Header = "Copy value" };
            copyValue.Click += (s, ev) =>
            {
                if (kv != null) Clipboard.SetText(kv.Value ?? string.Empty);
            };
            menu.Items.Add(copyValue);

            var copyTable = new MenuItem { Header = "Copy table" };
            copyTable.Click += (s, ev) => CopyAllAsTable();
            menu.Items.Add(copyTable);

            // if right-click wasn't directly on an item, we still show the menu so user can copy table
            menu.IsOpen = true;
            e.Handled = true;
        }

        private void ItemsHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var el = e.OriginalSource as FrameworkElement;
            var kv = el?.DataContext as Models.ListViewItem;
                if (kv != null)
            {
                // copy the value to clipboard and provide visual feedback via flash
                Clipboard.SetText(kv.Value ?? string.Empty);
                // TODO: visual feedback - maybe flash background; keep simple for now
                e.Handled = true;
            }
        }

        private void CopyAllAsTable()
        {
            try
            {
                if (ItemsHost.ItemsSource is System.Collections.IEnumerable items)
                {
                    var lines = new System.Text.StringBuilder();
                    foreach (var o in items)
                    {
                        if (o is Models.ListViewItem kv)
                        {
                            // tab separated key and value, value will be empty string if null
                            lines.Append(kv.Key ?? string.Empty).Append('\t').Append(kv.Value ?? string.Empty).AppendLine();
                        }
                    }
                    var s = lines.ToString();
                    if (!string.IsNullOrEmpty(s)) Clipboard.SetText(s);
                }
                else
                {
                    // fallback: copy nothing
                }
            }
            catch { }
        }
    }
}
