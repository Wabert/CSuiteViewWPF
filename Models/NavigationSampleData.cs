using System.Collections.ObjectModel;

namespace CSuiteViewWPF.Models
{
    public class NavigationNode
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ObservableCollection<NavigationNode> Children { get; set; } = new();
    }

    public static class NavigationSampleData
    {
        public static ObservableCollection<NavigationNode> GetNodes()
        {
            // Provide minimal sample data so the designer and app can load without Archive types
            var root = new ObservableCollection<NavigationNode>
            {
                new NavigationNode { Name = "Root", Type = "Folder", Children = new ObservableCollection<NavigationNode>
                    {
                        new NavigationNode { Name = "Child A", Type = "File" },
                        new NavigationNode { Name = "Child B", Type = "File" }
                    }
                }
            };
            return root;
        }
    }
}
