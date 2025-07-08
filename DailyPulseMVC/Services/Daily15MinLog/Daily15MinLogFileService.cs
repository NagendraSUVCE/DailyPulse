
using Models.DailyLog;
using DailyPulseMVC.Models; // Add this if StreakResult exists in the Models namespace.
using System.Data; // Required for DataTable

namespace DailyPulseMVC.Services
{

    public class Daily15MinLogFileService
    {
        public string StreakResultFileName { get; set; }
        private string _csvPath;
        private string _tempStreakResultFileName;
        private string folderPath;

        public Daily15MinLogFileService()
        {
            StreakResultFileName = "StreakResults.csv";
            _tempStreakResultFileName = $"temp_{StreakResultFileName}";

            _csvPath = $@"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/{StreakResultFileName}";
            folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
        }

        public async Task<DataTable> SaveStreakResultToFile(List<StreakResult> streakResults)
        {
            DataTable streakResultsDatatable = DataTableConverter.ToDataTable(streakResults);
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, StreakResultFileName, _tempStreakResultFileName);

/*
            DataTableConverter.DataTableToCsv(streakResultsDatatable
            , @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/"
            , "StreakResults.csv", false);
            */
            DataTableConverter.DataTableToCsv(streakResultsDatatable
            , ""
            , _tempStreakResultFileName, false);
            await UploadDataToGraphAsync();
               if (File.Exists(_tempStreakResultFileName))
            {
                File.Delete(_tempStreakResultFileName);
            }
            return streakResultsDatatable;
        }

        public async Task UploadDataToGraphAsync()
        {
            try
            {
                await GraphFileUtility.UploadFile(folderPath, StreakResultFileName, _tempStreakResultFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading data to Graph: {ex.Message}");
                throw ex;
            }
        }

    }
}