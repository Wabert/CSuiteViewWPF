using System;
namespace CSuiteViewWPF.Models
{
    public class ListViewItem
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
        public ListViewItem() { }
        public ListViewItem(string? k, string? v) { Key = k; Value = v; }
    }
}
