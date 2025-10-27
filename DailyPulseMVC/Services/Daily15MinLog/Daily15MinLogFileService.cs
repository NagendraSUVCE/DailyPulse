
using Models.DailyLog;
using DailyPulseMVC.Models; // Add this if StreakResult exists in the Models namespace.
using System.Data; // Required for DataTable
using System.Globalization; // Required for CultureInfo

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
        public async Task SaveLogSummaryForEachDayToFile(List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay)
        {
            string logSummaryForEachDayCSVFilePath = "DailyLogSummaryForEachDay.csv";
            string temp_logSummaryForEachDayCSVFilePath = $"temp_{logSummaryForEachDayCSVFilePath}";
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, logSummaryForEachDayCSVFilePath, temp_logSummaryForEachDayCSVFilePath);
            CreateCsvFromList(lstDailyLogSummaryForEachDay, temp_logSummaryForEachDayCSVFilePath);
            await GraphFileUtility.UploadFile(folderPath, logSummaryForEachDayCSVFilePath, temp_logSummaryForEachDayCSVFilePath);
            if (File.Exists(temp_logSummaryForEachDayCSVFilePath))
            {
                File.Delete(temp_logSummaryForEachDayCSVFilePath);
            }
        }
        private void CreateCsvFromList(List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Add headers
                writer.WriteLine("Category,ActivityDate,TotalValue");

                // Add data
                foreach (var expensesPendingObj in lstDailyLogSummaryForEachDay)
                {
                    var lineToAppend = $"{expensesPendingObj.Category}," +
                                         $"{expensesPendingObj.ActivityDate.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}," +
                                         $"{expensesPendingObj.TotalValue}";

                    writer.WriteLine(lineToAppend);
                }
            }
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