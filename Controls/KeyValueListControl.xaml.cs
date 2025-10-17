using CSuiteViewWPF.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSuiteViewWPF
{
    public partial class KeyValueListControl : UserControl
    {
        public KeyValueListControl()
        {
            InitializeComponent();
            // populate with sample data for now
            var sample = new List<KeyValueItem>
            {
                new KeyValueItem("First Name","John"),
                new KeyValueItem("Last Name","Doe"),
                new KeyValueItem("Street","1234 Main St"),
                new KeyValueItem("City","Springfield"),
                new KeyValueItem("State","IL"),
                new KeyValueItem("DOB","1985-03-12"),
                new KeyValueItem("Issue Date","2020-01-01"),
                new KeyValueItem("SSN","***-**-1234"),
                new KeyValueItem("Email","john.doe@example.com"),
                new KeyValueItem("Phone","(555) 123-4567"),
            };
            ItemsHost.ItemsSource = sample;

            // attach event to allow copying text on click
            ItemsHost.MouseLeftButtonUp += ItemsHost_MouseLeftButtonUp;
            ItemsHost.PreviewMouseRightButtonUp += ItemsHost_PreviewMouseRightButtonUp;
        }

        private void ItemsHost_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // show copy context menu for the Value under mouse
            var el = e.OriginalSource as FrameworkElement;
            var kv = el?.DataContext as KeyValueItem;
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
            var kv = el?.DataContext as KeyValueItem;
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
                        if (o is KeyValueItem kv)
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
