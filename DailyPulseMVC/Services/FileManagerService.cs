using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DailyPulseMVC.Services
{
    public class FileManagerService
    {
        public class FileDetails
        {
            public string FileName { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastModifiedDate { get; set; }
            public string FullPath { get; set; }
            public long SizeInBytes { get; set; }
        }

        public List<FileDetails> GetFilesFromFolder(string folderPath)
        {
            var fileDetailsList = new List<FileDetails>();

            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);

                    fileDetailsList.Add(new FileDetails
                    {
                        FileName = fileInfo.Name,
                        CreatedDate = fileInfo.CreationTime,
                        LastModifiedDate = fileInfo.LastWriteTime,
                        FullPath = fileInfo.FullName,
                        SizeInBytes = fileInfo.Length
                    });
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"The folder path '{folderPath}' does not exist.");
            }

            return fileDetailsList;
        }

        public List<(string OriginalFilePath, string DuplicateFilePath, long SizeInBytes)> FindDuplicateFiles(string folderPath)
        {
            var duplicates = new List<(string OriginalFilePath, string DuplicateFilePath, long SizeInBytes)>();
            var fileMetaData = new Dictionary<(string FileName, long SizeInBytes), string>(); // (FileName, SizeInBytes) -> FilePath

            if (Directory.Exists(folderPath))
            {
            var files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var key = (fileInfo.Name, fileInfo.Length);

                if (fileMetaData.ContainsKey(key))
                {
                duplicates.Add((fileMetaData[key], file, fileInfo.Length));
                }
                else
                {
                fileMetaData[key] = file;
                }
            }
            }
            else
            {
            throw new DirectoryNotFoundException($"The folder path '{folderPath}' does not exist.");
            }

            return duplicates;
        }
    }
}