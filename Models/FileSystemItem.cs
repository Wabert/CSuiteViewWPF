using System;

namespace CSuiteViewWPF.Models
{
    public class FileSystemItem
    {
        public string FullPath { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty; // e.g., "File", "Folder", ".lnk"
        public string ObjectName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long? Size { get; set; } // Nullable for folders
        public DateTime? DateLastModified { get; set; } // Nullable if not applicable
    }
}