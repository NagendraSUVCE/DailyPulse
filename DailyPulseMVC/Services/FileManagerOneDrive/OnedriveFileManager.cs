using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace FileManagerOneDrive
{
    public class OnedriveFileManager
    {
        public DataTable GetFolderSizes(string baseFolderPath)
        {
            if (string.IsNullOrEmpty(baseFolderPath) || !Directory.Exists(baseFolderPath))
                throw new ArgumentException("Invalid base folder path.");

            var folderSizes = new List<FolderInfo>();
            long totalSize = CalculateFolderSizes(baseFolderPath, folderSizes);

            // Sort folders by size in descending order and take the top 100
            var topFolders = folderSizes.OrderByDescending(f => f.TotalSize).Take(100).ToList();

            // Create DataTable
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Slno", typeof(int));
            dataTable.Columns.Add("Folder Name", typeof(string));
            dataTable.Columns.Add("Number of Files", typeof(int));
            dataTable.Columns.Add("Total Size (Bytes)", typeof(long));

            // Add rows for top 100 folders
            for (int i = 0; i < topFolders.Count; i++)
            {
                var folder = topFolders[i];
                dataTable.Rows.Add(i + 1, folder.FolderName, folder.FileCount, folder.TotalSize);
            }

            // Add a row for the total size of the base folder
            dataTable.Rows.Add("Total", baseFolderPath, folderSizes.Sum(f => f.FileCount), totalSize);

            return dataTable;
        }

        private long CalculateFolderSizes(string folderPath, List<FolderInfo> folderSizes)
        {
            long folderSize = 0;
            int fileCount = 0;

            // Get all files in the current folder
            var files = Directory.GetFiles(folderPath);
            fileCount += files.Length;
            folderSize += files.Sum(file => new FileInfo(file).Length);

            // Recursively calculate sizes for subfolders
            var subfolders = Directory.GetDirectories(folderPath);
            foreach (var subfolder in subfolders)
            {
                folderSize += CalculateFolderSizes(subfolder, folderSizes);
            }

            // Add folder info to the list
            folderSizes.Add(new FolderInfo
            {
                FolderName = folderPath,
                FileCount = fileCount,
                TotalSize = folderSize
            });

            return folderSize;
        }

        private class FolderInfo
        {
            public string FolderName { get; set; }
            public int FileCount { get; set; }
            public long TotalSize { get; set; }
        }
    }
}