using System;
using System.Data;

namespace CSuiteViewWPF.Models
{
    /// <summary>
    /// Helper methods for creating and populating DataTable instances that represent file system items.
    /// </summary>
    public static class FileSystemDataTableBuilder
    {
        /// <summary>
        /// Creates a DataTable with the schema expected by the filter grid.
        /// </summary>
        public static DataTable CreateTable()
        {
            var table = new DataTable("FileSystemItems");

            table.Columns.Add("FullPath", typeof(string));
            table.Columns.Add("ObjectType", typeof(string));
            table.Columns.Add("ObjectName", typeof(string));
            table.Columns.Add("FileExtension", typeof(string));

            var sizeColumn = table.Columns.Add("Size", typeof(long));
            sizeColumn.AllowDBNull = true;

            var dateColumn = table.Columns.Add("DateLastModified", typeof(DateTime));
            dateColumn.AllowDBNull = true;

            return table;
        }

        /// <summary>
        /// Adds a new row using raw values. Handles DBNull assignments for nullable fields.
        /// </summary>
        public static void AddRow(
            DataTable table,
            string? fullPath,
            string? objectType,
            string? objectName,
            string? fileExtension,
            long? size,
            DateTime? dateLastModified)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var row = table.NewRow();
            row["FullPath"] = fullPath ?? string.Empty;
            row["ObjectType"] = objectType ?? string.Empty;
            row["ObjectName"] = objectName ?? string.Empty;
            row["FileExtension"] = fileExtension ?? string.Empty;
            row["Size"] = size.HasValue ? size.Value : DBNull.Value;
            row["DateLastModified"] = dateLastModified.HasValue ? dateLastModified.Value : DBNull.Value;

            table.Rows.Add(row);
        }

        /// <summary>
        /// Adds a row based on a <see cref="FileSystemItem"/> instance.
        /// </summary>
        public static void AddRow(DataTable table, FileSystemItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            AddRow(
                table,
                item.FullPath,
                item.ObjectType,
                item.ObjectName,
                item.FileExtension,
                item.Size,
                item.DateLastModified);
        }
    }
}
