using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF
{
    public partial class TableDisplayControl : UserControl
    {
        // DependencyProperty to allow binding an ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(TableDisplayControl), new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TableDisplayControl ctl)
            {
                ctl.ApplyItemsSource(e.NewValue);
            }
        }

        // DependencyProperty to allow callers to request auto-generated columns
        public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register(
            nameof(AutoGenerateColumns), typeof(bool), typeof(TableDisplayControl), new PropertyMetadata(false));

        public bool AutoGenerateColumns
        {
            get => (bool)GetValue(AutoGenerateColumnsProperty);
            set => SetValue(AutoGenerateColumnsProperty, value);
        }

        public TableDisplayControl()
        {
            InitializeComponent();
            DataGridHost.PreviewMouseRightButtonUp += DataGridHost_PreviewMouseRightButtonUp;
        }

        private void ApplyItemsSource(object? source)
        {
            if (source == null)
            {
                DataGridHost.ItemsSource = null;
                return;
            }

            if (AutoGenerateColumns)
            {
                DataGridHost.Columns.Clear();
                DataGridHost.AutoGenerateColumns = true;
                DataGridHost.ItemsSource = source as IEnumerable;
            }
            else
            {
                DataGridHost.AutoGenerateColumns = false;
                DataGridHost.ItemsSource = source as IEnumerable;
            }
        }

        // Public helper to display an IEnumerable (rows can be POCOs or dictionaries)
        public void DisplayData(IEnumerable data, bool autoGenerateColumns = false)
        {
            AutoGenerateColumns = autoGenerateColumns;
            ItemsSource = data;
        }

        // Public helper to display a DataTable
        public void DisplayData(DataTable table)
        {
            AutoGenerateColumns = true;
            DataGridHost.ItemsSource = table.DefaultView;
        }

        // Expose copy all as table publicly
        public void CopyAllAsTablePublic() => CopyAllAsTable();

        private void DataGridHost_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;

            // walk up the visual tree to find the DataGridRow or DataGridCell
            while (dep != null && !(dep is DataGridCell) && !(dep is DataGridRow))
                dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);

            var menu = new ContextMenu();

            var copyCell = new MenuItem { Header = "Copy value" };
            copyCell.Click += (s, ev) =>
            {
                if (dep is DataGridCell cell)
                {
                    if (cell.Content is TextBlock tb) Clipboard.SetText(tb.Text ?? string.Empty);
                }
            };
            menu.Items.Add(copyCell);

            var copyTable = new MenuItem { Header = "Copy table" };
            copyTable.Click += (s, ev) => CopyAllAsTable();
            menu.Items.Add(copyTable);

            menu.IsOpen = true;
            e.Handled = true;
        }

        private void CopyAllAsTable()
        {
            try
            {
                if (DataGridHost.ItemsSource is System.Collections.IEnumerable items)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var o in items)
                    {
                        if (o is Models.TableRow r)
                        {
                            sb.Append(r.Phs).Append('\t')
                              .Append(r.Form ?? string.Empty).Append('\t')
                              .Append(r.Plancode ?? string.Empty).Append('\t')
                              .Append(r.IssueDate ?? string.Empty).Append('\t')
                              .Append(r.Amount ?? string.Empty).Append('\t')
                              .Append(r.IssAge).Append('\t')
                              .Append(r.Gender ?? string.Empty).Append('\t')
                              .Append(r.Class ?? string.Empty).AppendLine();
                        }
                    }

                    var s = sb.ToString();
                    if (!string.IsNullOrEmpty(s)) Clipboard.SetText(s);
                }
            }
            catch { }
        }
    }
}
