using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace DailyPulseMVC.Services.Diary
{
    public class DiaryService
    {
        public string diaryFileName { get; set; }
        private string _csvPath;
        private string _tempdiaryFileName;
        private string folderPath;
        private string _filePath;
        private List<DiaryEntry> _entries = new List<DiaryEntry>();

        public DiaryService()
        {
            diaryFileName = "diary.json";
            _csvPath = $@"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/Diary/{diaryFileName}";
            _tempdiaryFileName = $"temp_{diaryFileName}";
            folderPath = @"Nagendra/SelfCode/DatabaseInCSV/Diary";
            _filePath = _csvPath;
        }
        public async Task SaveEntries(IEnumerable<DiaryEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            _filePath = _tempdiaryFileName;
            var json = JsonSerializer.Serialize(entries);
            if (_filePath.Contains("/"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            }
            System.IO.File.WriteAllText(_filePath, json);
            await GraphFileUtility.UploadFile(folderPath, diaryFileName, _tempdiaryFileName);
            if (File.Exists(_tempdiaryFileName))
            {
                File.Delete(_tempdiaryFileName);
            }
        }

        public async Task<List<DiaryEntry>> LoadEntries()
        {

            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, diaryFileName, _tempdiaryFileName);
            _filePath = _tempdiaryFileName;
            if (!System.IO.File.Exists(_filePath))
                return new List<DiaryEntry>();
            var json = System.IO.File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<DiaryEntry>();

            if (File.Exists(_tempdiaryFileName))
            {
                File.Delete(_tempdiaryFileName);
            }
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json)?.OrderByDescending(entry => entry.LoggedTime).ToList() ?? new List<DiaryEntry>();
        }
    }
}