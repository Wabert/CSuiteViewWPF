using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// Generates test data for performance testing the high-performance filtering system.
    /// Can generate datasets with 300k+ rows for stress testing.
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random _random = new Random(42); // Fixed seed for reproducibility

        private static readonly string[] FileExtensions = new[]
        {
            ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".png", ".gif", ".bmp", ".mp4", ".avi", ".mp3", ".wav",
            ".zip", ".rar", ".7z", ".tar", ".cs", ".js", ".py", ".java",
            ".html", ".css", ".xml", ".json", ".sql", ".exe", ".dll", ".log"
        };

        private static readonly string[] ObjectTypes = new[]
        {
            "File", "Folder", ".lnk", "Archive", "Document", "Image", "Video", "Audio"
        };

        private static readonly string[] FolderNames = new[]
        {
            "Documents", "Downloads", "Pictures", "Videos", "Music", "Desktop",
            "Projects", "Work", "Personal", "Archive", "Backup", "Temp",
            "Development", "Resources", "Reports", "Data", "Config", "Logs"
        };

        private static readonly string[] FileNamePrefixes = new[]
        {
            "Report", "Document", "Image", "Photo", "Video", "Audio", "Backup",
            "Project", "File", "Data", "Config", "Log", "Archive", "Temp",
            "Draft", "Final", "Copy", "Original", "Updated", "Modified"
        };

        /// <summary>
        /// Generates a large dataset of FileSystemItem objects for performance testing
        /// </summary>
        /// <param name="rowCount">Number of rows to generate (e.g., 300000)</param>
        /// <param name="distinctValuesPerColumn">Controls variety - lower = more duplicates (better for filter testing)</param>
        /// <returns>ObservableCollection of generated FileSystemItem objects</returns>
        public static ObservableCollection<FileSystemItem> GenerateLargeDataset(
            int rowCount = 300000,
            int distinctValuesPerColumn = 1000)
        {
            var sw = Stopwatch.StartNew();
            Debug.WriteLine($"[TestDataGenerator] Generating {rowCount:N0} rows with ~{distinctValuesPerColumn} distinct values per column...");

            var items = new ObservableCollection<FileSystemItem>();

            // Pre-generate distinct values for better performance
            var distinctFullPaths = GenerateDistinctPaths(distinctValuesPerColumn);
            var distinctObjectNames = GenerateDistinctObjectNames(distinctValuesPerColumn);

            for (int i = 0; i < rowCount; i++)
            {
                items.Add(GenerateFileSystemItem(i, distinctFullPaths, distinctObjectNames));

                // Progress reporting
                if (i > 0 && i % 50000 == 0)
                {
                    Debug.WriteLine($"[TestDataGenerator] Generated {i:N0} rows...");
                }
            }

            sw.Stop();
            Debug.WriteLine($"[TestDataGenerator] Generated {rowCount:N0} rows in {sw.ElapsedMilliseconds:N0}ms " +
                          $"({rowCount / (sw.ElapsedMilliseconds / 1000.0):N0} rows/sec)");

            return items;
        }

        /// <summary>
        /// Generates a FileSystemItem with realistic but randomized data
        /// </summary>
        private static FileSystemItem GenerateFileSystemItem(
            int index,
            List<string> distinctPaths,
            List<string> distinctNames)
        {
            // Randomly select object type (80% files, 15% folders, 5% shortcuts)
            var typeRoll = _random.Next(100);
            var objectType = typeRoll < 80 ? "File" : typeRoll < 95 ? "Folder" : ".lnk";

            // Select a random path and name
            var fullPath = distinctPaths[_random.Next(distinctPaths.Count)];
            var objectName = distinctNames[_random.Next(distinctNames.Count)];

            // For files, add extension
            var fileExtension = string.Empty;
            if (objectType == "File")
            {
                fileExtension = FileExtensions[_random.Next(FileExtensions.Length)];
                objectName += fileExtension;
            }

            // Combine path and name
            fullPath = $"{fullPath}\\{objectName}";

            // Generate size (null for folders, random for files)
            long? size = null;
            if (objectType == "File")
            {
                // Generate realistic file sizes (most files are small, some are large)
                var sizeCategory = _random.Next(100);
                if (sizeCategory < 50)
                {
                    // Small files: 1 KB - 100 KB
                    size = _random.Next(1024, 100 * 1024);
                }
                else if (sizeCategory < 80)
                {
                    // Medium files: 100 KB - 10 MB
                    size = _random.Next(100 * 1024, 10 * 1024 * 1024);
                }
                else if (sizeCategory < 95)
                {
                    // Large files: 10 MB - 100 MB
                    size = _random.Next(10 * 1024 * 1024, 100 * 1024 * 1024);
                }
                else
                {
                    // Very large files: 100 MB - 1 GB
                    size = _random.Next(100 * 1024 * 1024, 1024 * 1024 * 1024);
                }
            }

            // Generate modification date (within last 2 years)
            var daysAgo = _random.Next(0, 730);
            var dateModified = DateTime.Now.AddDays(-daysAgo).AddHours(_random.Next(0, 24));

            return new FileSystemItem
            {
                FullPath = fullPath,
                ObjectType = objectType,
                ObjectName = objectName,
                FileExtension = fileExtension,
                Size = size,
                DateLastModified = dateModified
            };
        }

        /// <summary>
        /// Generates a list of distinct file/folder paths
        /// </summary>
        private static List<string> GenerateDistinctPaths(int count)
        {
            var paths = new List<string>();
            var basePath = "C:\\Users\\TestUser";

            for (int i = 0; i < count; i++)
            {
                var depth = _random.Next(1, 5); // 1-4 levels deep
                var pathParts = new List<string> { basePath };

                for (int d = 0; d < depth; d++)
                {
                    pathParts.Add(FolderNames[_random.Next(FolderNames.Length)] + (i % 100 == 0 ? $"_{i / 100}" : ""));
                }

                paths.Add(string.Join("\\", pathParts));
            }

            return paths;
        }

        /// <summary>
        /// Generates a list of distinct object names (without extensions)
        /// </summary>
        private static List<string> GenerateDistinctObjectNames(int count)
        {
            var names = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var prefix = FileNamePrefixes[_random.Next(FileNamePrefixes.Length)];
                var suffix = _random.Next(1000, 9999);
                var year = _random.Next(2020, 2026);

                // Mix different naming patterns
                var pattern = _random.Next(5);
                var name = pattern switch
                {
                    0 => $"{prefix}_{suffix}",
                    1 => $"{prefix}_{year}_{suffix}",
                    2 => $"{prefix}_{year}",
                    3 => $"{prefix}_{i % 1000}",
                    _ => $"{prefix}{suffix}"
                };

                names.Add(name);
            }

            return names;
        }

        /// <summary>
        /// Generates a small dataset for quick testing
        /// </summary>
        public static ObservableCollection<FileSystemItem> GenerateSmallDataset(int rowCount = 1000)
        {
            return GenerateLargeDataset(rowCount, distinctValuesPerColumn: 100);
        }

        /// <summary>
        /// Generates a dataset with specific characteristics for testing filter performance
        /// </summary>
        public static ObservableCollection<FileSystemItem> GenerateFilterTestDataset(
            int rowCount = 300000,
            int distinctExtensions = 20,
            int distinctObjectTypes = 5,
            int distinctFolders = 50)
        {
            Debug.WriteLine($"[TestDataGenerator] Generating filter test dataset: {rowCount:N0} rows");

            // Use smaller variety to stress-test filtering (more duplicates = more rows per filter value)
            return GenerateLargeDataset(rowCount, distinctValuesPerColumn: Math.Max(distinctFolders, 50));
        }

        /// <summary>
        /// Prints statistics about a dataset (useful for verifying data quality)
        /// </summary>
        public static void PrintDatasetStats(ObservableCollection<FileSystemItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Debug.WriteLine("[TestDataGenerator] Dataset is empty");
                return;
            }

            Debug.WriteLine("=== Dataset Statistics ===");
            Debug.WriteLine($"Total rows: {items.Count:N0}");
            Debug.WriteLine($"Distinct FullPaths: {items.Select(i => i.FullPath).Distinct().Count():N0}");
            Debug.WriteLine($"Distinct ObjectTypes: {items.Select(i => i.ObjectType).Distinct().Count():N0}");
            Debug.WriteLine($"Distinct ObjectNames: {items.Select(i => i.ObjectName).Distinct().Count():N0}");
            Debug.WriteLine($"Distinct FileExtensions: {items.Select(i => i.FileExtension).Distinct().Count():N0}");

            var filesWithSize = items.Where(i => i.Size.HasValue).ToList();
            if (filesWithSize.Any())
            {
                Debug.WriteLine($"Files with size: {filesWithSize.Count:N0}");
                Debug.WriteLine($"Average size: {filesWithSize.Average(i => i.Size!.Value):N0} bytes");
                Debug.WriteLine($"Min size: {filesWithSize.Min(i => i.Size!.Value):N0} bytes");
                Debug.WriteLine($"Max size: {filesWithSize.Max(i => i.Size!.Value):N0} bytes");
            }

            var itemsWithDate = items.Where(i => i.DateLastModified.HasValue).ToList();
            if (itemsWithDate.Any())
            {
                Debug.WriteLine($"Items with date: {itemsWithDate.Count:N0}");
                Debug.WriteLine($"Oldest: {itemsWithDate.Min(i => i.DateLastModified!.Value):yyyy-MM-dd}");
                Debug.WriteLine($"Newest: {itemsWithDate.Max(i => i.DateLastModified!.Value):yyyy-MM-dd}");
            }

            Debug.WriteLine("========================");
        }
    }
}
