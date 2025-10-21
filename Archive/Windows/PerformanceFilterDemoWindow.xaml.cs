using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CSuiteViewWPF.Services;
using CSuiteViewWPF.ViewModels;

namespace CSuiteViewWPF.Windows
{
    /// <summary>
    /// Demonstration window for the high-performance filtering system.
    /// Shows how to handle 300,000+ rows with sub-100ms filter updates.
    /// </summary>
    public partial class PerformanceFilterDemoWindow : Window
    {
        private PerformantFilteredDataGridViewModel? _viewModel;

        public PerformanceFilterDemoWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            UpdateStatus("Ready. Generate test data to begin.");
        }

        private void InitializeViewModel()
        {
            _viewModel = new PerformantFilteredDataGridViewModel();
            FilteredGrid.DataContext = _viewModel;
            
            // Subscribe to property changes to update status
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PerformantFilteredDataGridViewModel.StatusMessage))
                {
                    UpdateStatus(_viewModel.StatusMessage);
                }
                else if (e.PropertyName == nameof(PerformantFilteredDataGridViewModel.PerformanceStats))
                {
                    UpdatePerformanceStats(_viewModel.PerformanceStats);
                }
            };
        }

        #region Data Generation Event Handlers

        private async void GenerateSmallData_Click(object sender, RoutedEventArgs e)
        {
            await GenerateAndLoadDataAsync(10000, "Small (10k rows)");
        }

        private async void GenerateMediumData_Click(object sender, RoutedEventArgs e)
        {
            await GenerateAndLoadDataAsync(100000, "Medium (100k rows)");
        }

        private async void GenerateLargeData_Click(object sender, RoutedEventArgs e)
        {
            await GenerateAndLoadDataAsync(300000, "Large (300k rows)");
        }

        private async void GenerateXLData_Click(object sender, RoutedEventArgs e)
        {
            await GenerateAndLoadDataAsync(500000, "Extra Large (500k rows)");
        }

        private void ClearData_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.ClearData();
            LoadStatusText.Text = "Data cleared.";
            UpdateStatus("No data loaded.");
            UpdatePerformanceStats("");
            
            // Clear performance history
            PerformanceTimer.ClearHistory();
        }

        #endregion

        #region Data Loading

        private async Task GenerateAndLoadDataAsync(int rowCount, string description)
        {
            if (_viewModel == null) return;

            try
            {
                // Disable buttons during generation
                SetButtonsEnabled(false);
                
                LoadStatusText.Text = $"Generating {description}...";
                UpdateStatus($"Generating {rowCount:N0} test records...");

                var sw = Stopwatch.StartNew();

                // Generate data on background thread
                var data = await Task.Run(() =>
                {
                    using (var timer = new PerformanceTimer($"Generate {rowCount:N0} rows"))
                    {
                        return TestDataGenerator.GenerateLargeDataset(rowCount, distinctValuesPerColumn: 100);
                    }
                });

                sw.Stop();
                var generateTime = sw.ElapsedMilliseconds;

                // Load data and build indexes
                LoadStatusText.Text = $"Building indexes for {description}...";
                UpdateStatus($"Building indexes for {rowCount:N0} rows...");

                sw.Restart();
                
                using (var timer = new PerformanceTimer($"Load and Index {rowCount:N0} rows"))
                {
                    _viewModel.LoadItems(data);
                }

                sw.Stop();
                var loadTime = sw.ElapsedMilliseconds;

                // Update UI
                LoadStatusText.Text = $"✅ {description} loaded successfully! " +
                    $"Generate: {generateTime:N0}ms, Load & Index: {loadTime:N0}ms";

                UpdatePerformanceStats(_viewModel.PerformanceStats);

                // Update filter indicators
                FilteredGrid.UpdateFilterIndicators();

                Debug.WriteLine($"Total time: {generateTime + loadTime:N0}ms");
            }
            catch (Exception ex)
            {
                LoadStatusText.Text = $"❌ Error: {ex.Message}";
                MessageBox.Show($"Error generating data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        #endregion

        #region Benchmark and Statistics

        private async void RunBenchmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetButtonsEnabled(false);
                LoadStatusText.Text = "Running comprehensive benchmark...";
                UpdateStatus("Benchmark in progress...");

                var results = await Task.Run(() =>
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("=== Performance Benchmark ===\n");
                    
                    // Test small dataset
                    var sw = Stopwatch.StartNew();
                    var small = TestDataGenerator.GenerateSmallDataset(1000);
                    sw.Stop();
                    sb.AppendLine($"Small (1K rows): {sw.ElapsedMilliseconds}ms");
                    
                    // Test medium dataset
                    sw.Restart();
                    var medium = TestDataGenerator.GenerateLargeDataset(50000, 500);
                    sw.Stop();
                    sb.AppendLine($"Medium (50K rows): {sw.ElapsedMilliseconds}ms");
                    
                    // Test large dataset
                    sw.Restart();
                    var large = TestDataGenerator.GenerateLargeDataset(300000, 1000);
                    sw.Stop();
                    sb.AppendLine($"Large (300K rows): {sw.ElapsedMilliseconds}ms");
                    
                    sb.AppendLine("\n=== Benchmark Complete ===");
                    return sb.ToString();
                });

                // Show results in message box
                var resultWindow = new Window
                {
                    Title = "Benchmark Results",
                    Width = 700,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Content = new ScrollViewer
                    {
                        Content = new TextBox
                        {
                            Text = results,
                            IsReadOnly = true,
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            FontSize = 12,
                            Padding = new Thickness(16),
                            TextWrapping = TextWrapping.NoWrap,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            BorderThickness = new Thickness(0)
                        }
                    }
                };

                resultWindow.ShowDialog();

                LoadStatusText.Text = "✅ Benchmark complete!";
                UpdateStatus("Benchmark complete. Check results window.");
            }
            catch (Exception ex)
            {
                LoadStatusText.Text = $"❌ Benchmark error: {ex.Message}";
                MessageBox.Show($"Error running benchmark: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void ShowPerformanceStats_Click(object sender, RoutedEventArgs e)
        {
            var summary = PerformanceTimer.GetSummary();
            
            var statsWindow = new Window
            {
                Title = "Performance Statistics",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = new ScrollViewer
                {
                    Content = new TextBox
                    {
                        Text = summary,
                        IsReadOnly = true,
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        FontSize = 12,
                        Padding = new Thickness(16),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        BorderThickness = new Thickness(0)
                    }
                }
            };

            statsWindow.ShowDialog();
        }

        #endregion

        #region UI Update Helpers

        private void UpdateStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.Text = message;
            }
        }

        private void UpdatePerformanceStats(string stats)
        {
            if (PerformanceStatsText != null)
            {
                PerformanceStatsText.Text = stats;
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            // Find all buttons in the window and enable/disable them
            foreach (var button in FindVisualChildren<System.Windows.Controls.Button>(this))
            {
                button.IsEnabled = enabled;
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) 
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject? child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    if (child != null)
                    {
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
