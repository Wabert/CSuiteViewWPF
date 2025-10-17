using System.Collections.ObjectModel;

namespace CSuiteViewWPF.Models
{
    public class TreeViewControlNode
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ObservableCollection<TreeViewControlNode> Children { get; set; } = new();
    }

    public static class TreeViewControlSampleData
    {
        public static ObservableCollection<TreeViewControlNode> GetNodes()
        {
            var prodTables = new ObservableCollection<TreeViewControlNode>
            {
                new TreeViewControlNode { Name = "Customers", Type = "Table" },
                new TreeViewControlNode { Name = "Orders", Type = "Table" },
                new TreeViewControlNode { Name = "Products", Type = "Table" },
                new TreeViewControlNode { Name = "Employees", Type = "Table" }
            };
            for (int i = 1; i <= 50; i++) prodTables.Add(new TreeViewControlNode { Name = $"Prod_Table_{i}", Type = "Table" });

            var devTables = new ObservableCollection<TreeViewControlNode>
            {
                new TreeViewControlNode { Name = "TestData", Type = "Table" },
                new TreeViewControlNode { Name = "Staging", Type = "Table" }
            };
            for (int i = 1; i <= 75; i++) devTables.Add(new TreeViewControlNode { Name = $"Dev_Table_{i}", Type = "Table" });

            return new ObservableCollection<TreeViewControlNode>
            {
                new TreeViewControlNode
                {
                    Name = "Database Connections",
                    Type = "Category",
                    Children = new ObservableCollection<TreeViewControlNode>
                    {
                        new TreeViewControlNode
                        {
                            Name = "Production Server",
                            Type = "Server",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode
                                {
                                    Name = "Tables",
                                    Type = "Folder",
                                    Children = prodTables
                                },
                                new TreeViewControlNode
                                {
                                    Name = "Views",
                                    Type = "Folder",
                                    Children = new ObservableCollection<TreeViewControlNode>
                                    {
                                        new TreeViewControlNode { Name = "CustomerOrders", Type = "View" },
                                        new TreeViewControlNode { Name = "ProductSales", Type = "View" }
                                    }
                                },
                                new TreeViewControlNode
                                {
                                    Name = "Stored Procedures",
                                    Type = "Folder",
                                    Children = new ObservableCollection<TreeViewControlNode>
                                    {
                                        new TreeViewControlNode { Name = "GetCustomerOrders", Type = "Stored Procedure" },
                                        new TreeViewControlNode { Name = "UpdateInventory", Type = "Stored Procedure" },
                                        new TreeViewControlNode { Name = "ProcessPayment", Type = "Stored Procedure" }
                                    }
                                }
                            }
                        },
                        new TreeViewControlNode
                        {
                            Name = "Development Server",
                            Type = "Server",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode
                                {
                                    Name = "Tables",
                                    Type = "Folder",
                                    Children = devTables
                                }
                            }
                        },
                        new TreeViewControlNode
                        {
                            Name = "Backup Server",
                            Type = "Server",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode
                                {
                                    Name = "Archives",
                                    Type = "Folder",
                                    Children = new ObservableCollection<TreeViewControlNode>
                                    {
                                        new TreeViewControlNode { Name = "2024_Q1", Type = "Archive" },
                                        new TreeViewControlNode { Name = "2024_Q2", Type = "Archive" },
                                        new TreeViewControlNode { Name = "2024_Q3", Type = "Archive" }
                                    }
                                }
                            }
                        }
                    }
                },
                new TreeViewControlNode
                {
                    Name = "File Systems",
                    Type = "Category",
                    Children = new ObservableCollection<TreeViewControlNode>
                    {
                        new TreeViewControlNode
                        {
                            Name = "Local Drive (C:)",
                            Type = "Drive",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode { Name = "Program Files", Type = "Folder" },
                                new TreeViewControlNode { Name = "Users", Type = "Folder" },
                                new TreeViewControlNode { Name = "Windows", Type = "Folder" }
                            }
                        },
                        new TreeViewControlNode
                        {
                            Name = "Network Shares",
                            Type = "Share",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode { Name = @"\\FileServer\Documents", Type = "Share" },
                                new TreeViewControlNode { Name = @"\\FileServer\Projects", Type = "Share" }
                            }
                        }
                    }
                },
                new TreeViewControlNode
                {
                    Name = "Projects",
                    Type = "Category",
                    Children = new ObservableCollection<TreeViewControlNode>
                    {
                        new TreeViewControlNode
                        {
                            Name = "CSuiteView",
                            Type = "Project",
                            Children = new ObservableCollection<TreeViewControlNode>
                            {
                                new TreeViewControlNode { Name = "Forms", Type = "Folder" },
                                new TreeViewControlNode { Name = "Models", Type = "Folder" },
                                new TreeViewControlNode { Name = "Managers", Type = "Folder" }
                            }
                        }
                    }
                }
            };
        }
    }
}
