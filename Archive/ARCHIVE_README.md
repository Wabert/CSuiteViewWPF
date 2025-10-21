# Archive Folder - October 20, 2025

This folder contains code and documentation that has been replaced or is no longer used in the active codebase.

## Archived Code Files

### ViewModels
- **FilteredDataGridViewModel.cs** - Replaced by `PerformantFilteredDataGridViewModel`
  - Old filtering implementation with individual column filter collections
  - Uses `FilterPerformanceLogger` and `FilterPerformanceMonitor`
  - Only throws `NotImplementedException` for new interface methods

- **FilterContentViewModel.cs** - Old ViewModel for FilterContent control
  - Replaced by direct usage in `SimpleFilterPopup`

### Services (Performance Monitoring - Only Used by Archived Code)
- **FilterPerformanceMonitor.cs** - Performance diagnostics class
  - Only used by `FilteredDataGridViewModel` (archived)
  
- **FilterPerformanceLogger.cs** - Performance logging utility
  - Only used by `FilteredDataGridViewModel` (archived)
  
- **PerformanceTimer.cs** - Timer utility for benchmarking
  - Only used by `PerformanceFilterDemoWindow` (archived)

### Controls (Old UI Components)
- **FilterContent.xaml / FilterContent.xaml.cs** - Old filter popup UI
  - Replaced by `SimpleFilterPopup.xaml / SimpleFilterPopup.xaml.cs`
  - Was used in now-archived code in `FilteredDataGridControl.xaml.cs`

### Windows (Demo/Testing)
- **PerformanceFilterDemoWindow.xaml / PerformanceFilterDemoWindow.xaml.cs** - Performance testing window
  - Used for load testing with 10K, 100K, 300K rows
  - References archived `PerformanceTimer` class
  - Accessible via "Test Windows" button (keep button for future use)

### Examples
- **PerformanceFilteringExamples.cs** - Code examples and demos
  - References archived `PerformanceTimer` class

---

## Archived Documentation Files

All documentation has been archived because it was becoming outdated with frequent iterations:

1. **TESTING_GUIDE.md** - Testing instructions for old FilteredDataGridViewModel
2. **SIMPLIFIED_FILTERING_CHANGES.md** - Documentation of SimpleFilterPopup changes
3. **REFACTORING_SUMMARY.md** - Summary of refactoring to generic filtering
4. **QUICK_START_PERFORMANCE_FILTERING.md** - Quick start guide
5. **PERFORMANCE_FILTERING_GUIDE.md** - Detailed performance guide
6. **IMPLEMENTATION_SUMMARY.md** - Implementation details
7. **HIGH_PERFORMANCE_FILTERING_README.md** - High-performance filtering readme
8. **FILTERING_IMPLEMENTATION.md** - Filtering implementation details  
9. **ARCHITECTURE_VISUAL_GUIDE.md** - Visual architecture guide

**Note:** Documentation will be recreated once the system is stable and fully tested.

---

## What's Still Active in Production

### Core Filtering System
- ✅ **`PerformantFilteredDataGridViewModel`** - High-performance filtering with BitArray indexing
- ✅ **`PerformantDataFilter<T>`** - Core filtering engine
- ✅ **`SimpleFilterPopup`** - New, simplified filter UI
- ✅ **`FilterPopupWindow`** - Window wrapper for filter popups
- ✅ **`FilteredDataGridControl`** - Main control (with archived code commented out)

### Application Windows
- ✅ **`FileSystemScannerWindow`** - Main production feature (Directory Scan button)
- ✅ **`WindowCreatorForTesting`** - Kept for future use
- ✅ **MainWindow** - Startup window with 3 buttons (all kept for future features)

### Other Active Code
- ✅ **`RelayCommand`** - Simple ICommand implementation (newly added)
- ✅ All Models, Converters, Resources, Services (except archived performance monitoring)

---

## Restoration Instructions

If you need to restore any archived code:

1. Copy the file(s) back to their original location
2. Remove any comment blocks in referencing code
3. Rebuild the project
4. Test functionality

---

## Build Status After Archiving

✅ **Build Successful**  
⚠️ 2 nullable reference warnings (non-critical, in TreeViewControl.xaml.cs)

**Files Archived:** 18 total (8 code files + 10 documentation files)  
**Lines of Code Removed from Active Codebase:** ~3,000+
