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
            // build Production Tables (50 dummy)
            var prodTables = new ObservableCollection<NavigationNode>
            {
                new NavigationNode { Name = "Customers", Type = "Table" },
                new NavigationNode { Name = "Orders", Type = "Table" },
                new NavigationNode { Name = "Products", Type = "Table" },
                new NavigationNode { Name = "Employees", Type = "Table" }
            };
            for (int i = 1; i <= 50; i++)
            {
                prodTables.Add(new NavigationNode { Name = $"Prod_Table_{i}", Type = "Table" });
            }

            // build Development Tables (75 dummy)
            var devTables = new ObservableCollection<NavigationNode>
            {
                new NavigationNode { Name = "TestData", Type = "Table" },
                new NavigationNode { Name = "Staging", Type = "Table" }
            };
            for (int i = 1; i <= 75; i++)
            {
                devTables.Add(new NavigationNode { Name = $"Dev_Table_{i}", Type = "Table" });
            }

            return new ObservableCollection<NavigationNode>
            {
                new NavigationNode
                {
                    Name = "Database Connections",
                    Type = "Category",
                    Children = new ObservableCollection<NavigationNode>
                    {
                        new NavigationNode
                        {
                            Name = "Production Server",
                            Type = "Server",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode
                                {
                                    Name = "Tables",
                                    Type = "Folder",
                                    Children = prodTables
                                },
                                new NavigationNode
                                {
                                    Name = "Views",
                                    Type = "Folder",
                                    Children = new ObservableCollection<NavigationNode>
                                    {
                                        new NavigationNode { Name = "CustomerOrders", Type = "View" },
                                        new NavigationNode { Name = "ProductSales", Type = "View" }
                                    }
                                },
                                new NavigationNode
                                {
                                    Name = "Stored Procedures",
                                    Type = "Folder",
                                    Children = new ObservableCollection<NavigationNode>
                                    {
                                        new NavigationNode { Name = "GetCustomerOrders", Type = "Stored Procedure" },
                                        new NavigationNode { Name = "UpdateInventory", Type = "Stored Procedure" },
                                        new NavigationNode { Name = "ProcessPayment", Type = "Stored Procedure" }
                                    }
                                }
                            }
                        },
                        new NavigationNode
                        {
                            Name = "Development Server",
                            Type = "Server",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode
                                {
                                    Name = "Tables",
                                    Type = "Folder",
                                    Children = devTables
                                }
                            }
                        },
                        new NavigationNode
                        {
                            Name = "Backup Server",
                            Type = "Server",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode
                                {
                                    Name = "Archives",
                                    Type = "Folder",
                                    Children = new ObservableCollection<NavigationNode>
                                    {
                                        new NavigationNode { Name = "2024_Q1", Type = "Archive" },
                                        new NavigationNode { Name = "2024_Q2", Type = "Archive" },
                                        new NavigationNode { Name = "2024_Q3", Type = "Archive" }
                                    }
                                }
                            }
                        }
                    }
                },
                new NavigationNode
                {
                    Name = "File Systems",
                    Type = "Category",
                    Children = new ObservableCollection<NavigationNode>
                    {
                        new NavigationNode
                        {
                            Name = "Local Drive (C:)",
                            Type = "Drive",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode { Name = "Program Files", Type = "Folder" },
                                new NavigationNode { Name = "Users", Type = "Folder" },
                                new NavigationNode { Name = "Windows", Type = "Folder" }
                            }
                        },
                        new NavigationNode
                        {
                            Name = "Network Shares",
                            Type = "Share",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode { Name = @"\\FileServer\Documents", Type = "Share" },
                                new NavigationNode { Name = @"\\FileServer\Projects", Type = "Share" }
                            }
                        }
                    }
                },
                new NavigationNode
                {
                    Name = "Projects",
                    Type = "Category",
                    Children = new ObservableCollection<NavigationNode>
                    {
                        new NavigationNode
                        {
                            Name = "CSuiteView",
                            Type = "Project",
                            Children = new ObservableCollection<NavigationNode>
                            {
                                new NavigationNode { Name = "Forms", Type = "Folder" },
                                new NavigationNode { Name = "Models", Type = "Folder" },
                                new NavigationNode { Name = "Managers", Type = "Folder" }
                            }
                        }
                    }
                }
            };
        }
    }
}
