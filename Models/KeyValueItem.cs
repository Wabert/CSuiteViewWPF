using System;
namespace CSuiteViewWPF.Models
{
    public class KeyValueItem
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
        public KeyValueItem() { }
        public KeyValueItem(string? k, string? v) { Key = k; Value = v; }
    }
}
