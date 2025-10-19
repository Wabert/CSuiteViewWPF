using System;
using System.Collections.ObjectModel;
using System.IO;
using CSuiteViewWPF.Models;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// Service for file system operations such as scanning directories.
    /// Separates file I/O logic from UI code.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Scans a directory and returns its contents as a collection of FileSystemItem objects.
        /// Includes both files and folders.
        /// </summary>
        /// <param name="path">The directory path to scan</param>
        /// <returns>Collection of FileSystemItem objects representing files and folders</returns>
        public ObservableCollection<FileSystemItem> ScanDirectory(string path)
        {
            var items = new ObservableCollection<FileSystemItem>();
            
            try
            {
                // Get all files in the directory
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        items.Add(new FileSystemItem
                        {
                            FullPath = fi.FullName,
                            ObjectType = "File",
                            ObjectName = fi.Name,
                            FileExtension = fi.Extension,
                            Size = fi.Length,
                            DateLastModified = fi.LastWriteTime
                        });
                    }
                    catch (Exception)
                    {
                        // If we can't access a specific file, add an error entry for it
                        items.Add(new FileSystemItem
                        {
                            FullPath = file,
                            ObjectType = "Error",
                            ObjectName = $"Cannot access file: {Path.GetFileName(file)}",
                            FileExtension = "",
                            Size = null,
                            DateLastModified = null
                        });
                    }
                }

                // Get all folders in the directory
                foreach (var folder in Directory.GetDirectories(path))
                {
                    try
                    {
                        var di = new DirectoryInfo(folder);
                        items.Add(new FileSystemItem
                        {
                            FullPath = di.FullName,
                            ObjectType = "Folder",
                            ObjectName = di.Name,
                            FileExtension = "",
                            Size = null,
                            DateLastModified = di.LastWriteTime
                        });
                    }
                    catch (Exception)
                    {
                        // If we can't access a specific folder, add an error entry for it
                        items.Add(new FileSystemItem
                        {
                            FullPath = folder,
                            ObjectType = "Error",
                            ObjectName = $"Cannot access folder: {Path.GetFileName(folder)}",
                            FileExtension = "",
                            Size = null,
                            DateLastModified = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // If the entire scan fails, add a general error entry
                items.Add(new FileSystemItem
                {
                    FullPath = "Error scanning directory",
                    ObjectType = "Error",
                    ObjectName = ex.Message,
                    FileExtension = "",
                    Size = null,
                    DateLastModified = null
                });
            }

            return items;
        }

        /// <summary>
        /// Gets the size of a directory by summing all file sizes.
        /// </summary>
        /// <param name="path">The directory path</param>
        /// <returns>Total size in bytes, or null if calculation fails</returns>
        public long? GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                long size = 0;
                
                // Get all files recursively
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }
                
                return size;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a path exists and is accessible.
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>True if path exists and is accessible, false otherwise</returns>
        public bool PathExists(string path)
        {
            try
            {
                return Directory.Exists(path) || File.Exists(path);
            }
            catch
            {
                return false;
            }
        }
    }
}
