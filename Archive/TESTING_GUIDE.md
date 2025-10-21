# Testing Guide for High-Performance Filtering

## Quick Start Testing

### 1. Generate Test Data

Add this code to your window or test file:

```csharp
using CSuiteViewWPF.Services;
using CSuiteViewWPF.ViewModels;

public class TestWindow : Window
{
    public TestWindow()
    {
        InitializeComponent();

        // Create ViewModel
        var viewModel = new FilteredDataGridViewModel();

        // Generate 300,000 test rows
        var testData = TestDataGenerator.GenerateLargeDataset(
            rowCount: 300000,
            distinctValuesPerColumn: 1000
        );

        // Print statistics to Debug output
        TestDataGenerator.PrintDatasetStats(testData);

        // Set data (this will build indexes in parallel)
        viewModel.Items = testData;

        // Set as DataContext
        DataContext = viewModel;
    }
}
```

### 2. Monitor Performance

Watch the **Debug Output** window in Visual Studio. You should see:

```
[TestDataGenerator] Generating 300,000 rows with ~1000 distinct values per column...
[TestDataGenerator] Generated 50,000 rows...
[TestDataGenerator] Generated 100,000 rows...
[TestDataGenerator] Generated 150,000 rows...
[TestDataGenerator] Generated 200,000 rows...
[TestDataGenerator] Generated 250,000 rows...
[TestDataGenerator] Generated 300,000 rows in 1,234ms (243,309 rows/sec)

=== Dataset Statistics ===
Total rows: 300,000
Distinct FullPaths: 1,000
Distinct ObjectTypes: 8
Distinct ObjectNames: 1,000
Distinct FileExtensions: 32
Files with size: 240,000
Average size: 52,428,800 bytes
Min size: 1,024 bytes
Max size: 1,073,741,824 bytes
Items with date: 300,000
Oldest: 2023-10-19
Newest: 2025-10-19
========================

[FilteredDataGridViewModel] Initialized filter engine with 300,000 rows, 6 columns indexed in 312ms
```

---

## Performance Testing Scenarios

### Test 1: Single Column Filter

1. Click on "Object Type" column header
2. Uncheck "Folder" (leave File and .lnk checked)
3. Click OK

**Expected:**
- Filter applies in <50ms
- Row count updates to show ~240,000 of 300,000 rows
- DataGrid updates instantly

**Debug Output:**
```
[FilteredDataGridViewModel] Applied all filters in 42ms
[FilteredDataGridViewModel] Updated ViewSource: 240,000 rows, 15ms
```

### Test 2: Multiple Column Filters

1. Filter "Object Type" to only "File"
2. Then filter "File Extension" to only ".txt", ".pdf", ".doc"
3. Then filter "Size" to a few values

**Expected:**
- Each filter applies in <80ms
- Row count shows progressive reduction
- UI remains responsive

**Debug Output:**
```
[FilteredDataGridViewModel] Applied all filters in 67ms
[FilteredDataGridViewModel] Updated ViewSource: 42,531 rows, 12ms
```

### Test 3: Clear Filters

1. With multiple filters active, click "Clear Filter" on one column
2. Observe the row count increase

**Expected:**
- Clearing takes <50ms
- Filter dropdown repopulates with all values
- Other filters remain active

### Test 4: Sort Large Dataset

1. With filters active, click "↑" to sort ascending
2. Or click "↓" to sort descending

**Expected:**
- Sort + index rebuild takes <600ms
- Sorted data appears correctly
- Filters remain active after sorting

---

## Stress Testing

### Test with Different Data Sizes

```csharp
// Small dataset (1,000 rows) - should be instant
var smallData = TestDataGenerator.GenerateSmallDataset(1000);

// Medium dataset (50,000 rows) - should be very fast
var mediumData = TestDataGenerator.GenerateLargeDataset(50000);

// Large dataset (300,000 rows) - target performance
var largeData = TestDataGenerator.GenerateLargeDataset(300000);

// Extra large dataset (500,000 rows) - stress test
var extraLargeData = TestDataGenerator.GenerateLargeDataset(500000);
```

### Test with Different Distinct Value Counts

```csharp
// Low variety (100 distinct values per column) - many duplicates
// This tests filter performance when each value appears in many rows
var lowVariety = TestDataGenerator.GenerateLargeDataset(
    rowCount: 300000,
    distinctValuesPerColumn: 100
);

// High variety (10,000 distinct values per column) - few duplicates
// This tests memory usage and filter dropdown performance
var highVariety = TestDataGenerator.GenerateLargeDataset(
    rowCount: 300000,
    distinctValuesPerColumn: 10000
);
```

---

## Performance Benchmarks

### Expected Results (4-core CPU, 16GB RAM)

| Operation | Dataset Size | Expected Time | Pass/Fail Criteria |
|-----------|--------------|---------------|---------------------|
| Index Building (Parallel) | 300k rows | 200-400ms | ✅ <500ms |
| Single Filter | 300k rows | 30-50ms | ✅ <100ms |
| Multiple Filters (3) | 300k rows | 50-80ms | ✅ <100ms |
| Clear Filter | 300k rows | 20-40ms | ✅ <100ms |
| Sort + Rebuild | 300k rows | 300-600ms | ✅ <1000ms |
| Open Filter Dropdown | Any size | 10-30ms | ✅ <50ms |

### Memory Usage Benchmarks

| Dataset Size | Distinct Values/Col | Expected Memory | Pass/Fail |
|--------------|---------------------|-----------------|-----------|
| 100k rows | 500 | ~40 MB | ✅ <100 MB |
| 300k rows | 1000 | ~120 MB | ✅ <200 MB |
| 300k rows | 5000 | ~400 MB | ⚠️ High but OK |
| 500k rows | 1000 | ~200 MB | ✅ <300 MB |

---

## Debugging Performance Issues

### Enable Detailed Logging

The filtering engine already logs to Debug output. To see all logs:

1. Open Visual Studio
2. Go to View → Output
3. Select "Debug" from the "Show output from" dropdown
4. Run your application
5. Perform filter operations

### Generate Performance Report

```csharp
// After using filters for a while
var viewModel = (FilteredDataGridViewModel)DataContext;
string report = viewModel.GetPerformanceReport();

// Show in a message box or log to file
MessageBox.Show(report);
// or
System.IO.File.WriteAllText("performance_report.txt", report);
```

### Check Specific Metrics

```csharp
var perfMonitor = viewModel.PerformanceMonitor;

// Get stats for a specific operation
var filterStats = perfMonitor.GetStats("ApplyFilters");
Debug.WriteLine($"ApplyFilters average: {filterStats.AverageDurationMs:F2}ms");

// Get recent operations
var recentMetrics = perfMonitor.GetRecentMetrics(10);
foreach (var metric in recentMetrics)
{
    Debug.WriteLine($"{metric.Operation}: {metric.DurationMs}ms - {metric.Details}");
}
```

---

## Common Issues and Solutions

### Issue: "Filter takes >100ms"

**Diagnosis:**
```
[PERFORMANCE WARNING] ApplyFilters took 156ms (target: <100ms) - 5 active filters
```

**Solutions:**
1. Check if you have too many active filters (>5)
2. Check if columns have very high distinct value counts (>10,000)
3. Verify indexes were built in parallel (check Debug output on startup)

### Issue: "High memory usage"

**Diagnosis:**
Task Manager shows >500 MB memory usage

**Solutions:**
1. Check distinct value counts with `PrintDatasetStats()`
2. Reduce `distinctValuesPerColumn` parameter in test data generation
3. Don't index columns with very high cardinality (>10,000 distinct values)

### Issue: "UI freezes during filtering"

**Solutions:**
1. Verify DataGrid virtualization is enabled (it is by default)
2. Check Debug output for exceptionally slow operations
3. Ensure you're not running in Debug mode (Release mode is faster)

### Issue: "Filter dropdown is slow to open"

**Diagnosis:**
Clicking header takes >200ms to show dropdown

**Solutions:**
1. Check distinct value count for that column
2. If >5,000 distinct values, consider using text search instead of checkbox list
3. Verify `GetDistinctValues()` is using indexed values (not scanning source data)

---

## Sample Test Window XAML

```xaml
<Window x:Class="CSuiteViewWPF.TestWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:CSuiteViewWPF.Controls"
        Title="Filter Performance Test" Height="600" Width="1200">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Test Controls -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Generate 300k Rows" Click="Generate300k_Click" Margin="5"/>
            <Button Content="Clear Filters" Click="ClearFilters_Click" Margin="5"/>
            <Button Content="Performance Report" Click="ShowReport_Click" Margin="5"/>
        </StackPanel>

        <!-- Filtered DataGrid -->
        <controls:FilteredDataGridControl Grid.Row="1" Margin="10"/>

        <!-- Status Bar -->
        <Border Grid.Row="2" Background="#27408B" Padding="10,5">
            <TextBlock Text="{Binding RowCountDisplay}"
                       Foreground="#FFD700"
                       FontWeight="Bold"
                       FontSize="14"/>
        </Border>
    </Grid>
</Window>
```

### Code-Behind for Test Window

```csharp
private void Generate300k_Click(object sender, RoutedEventArgs e)
{
    var vm = (FilteredDataGridViewModel)DataContext;
    var testData = TestDataGenerator.GenerateLargeDataset(300000);
    vm.Items = testData;
}

private void ClearFilters_Click(object sender, RoutedEventArgs e)
{
    var vm = (FilteredDataGridViewModel)DataContext;
    vm.RebuildAllFilters();
}

private void ShowReport_Click(object sender, RoutedEventArgs e)
{
    var vm = (FilteredDataGridViewModel)DataContext;
    string report = vm.GetPerformanceReport();
    MessageBox.Show(report, "Performance Report");
}
```

---

## Automated Testing

### Unit Test Example (if using MSTest or xUnit)

```csharp
[TestMethod]
public void FilterPerformance_300kRows_ShouldBeFast()
{
    // Arrange
    var testData = TestDataGenerator.GenerateLargeDataset(300000, 1000);
    var filterEngine = new PerformantDataFilter<FileSystemItem>(testData);
    filterEngine.BuildAllIndexesParallel("ObjectType", "FileExtension");

    // Act
    var sw = Stopwatch.StartNew();
    filterEngine.SetFilter("ObjectType", new HashSet<object> { "File" });
    sw.Stop();

    // Assert
    Assert.IsTrue(sw.ElapsedMilliseconds < 100,
        $"Filter took {sw.ElapsedMilliseconds}ms, expected <100ms");
}
```

---

## Visual Verification

When testing, verify:

1. ✅ **Row count display** updates correctly
   - Shows "Showing X of Y rows"
   - Updates immediately when filter applied

2. ✅ **Filter indicators** (underlines) appear on filtered columns
   - Underline appears when filter active
   - Underline removed when filter cleared

3. ✅ **Filter dropdown** shows correct values
   - Only shows values from visible rows (after other filters)
   - "(empty)" appears for null/empty values
   - Values are sorted correctly

4. ✅ **Checkbox state** persists
   - Unchecking values and clicking OK applies filter
   - Clicking Cancel restores original selections
   - Clear Filter selects all values

5. ✅ **DataGrid virtualization** works
   - Scrolling is smooth even with 300k rows
   - Only visible rows are rendered (check with Visual Studio performance profiler)

---

## Performance Profiling

### Using Visual Studio Profiler

1. Debug → Performance Profiler
2. Select "CPU Usage" and "Memory Usage"
3. Click Start
4. Perform filter operations in your app
5. Click Stop
6. Review the results:
   - `SetFilter()` should take <100ms
   - `BitArray.And()` should be the hottest method (this is normal and fast)
   - No memory leaks when applying/clearing filters repeatedly

---

## Success Criteria

Your implementation is successful if:

- ✅ Filtering 300k rows takes <100ms
- ✅ Index building takes <500ms (parallel)
- ✅ Memory usage is <200MB for 300k rows
- ✅ UI never freezes during filter operations
- ✅ Multiple filters can be applied/removed repeatedly
- ✅ Sorting works correctly with active filters
- ✅ Row count display is accurate
- ✅ All Debug output shows acceptable timings

---

**Ready to Test!** 🚀

Start by generating 300k rows and applying filters. Watch the Debug output for performance metrics.
