using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DailyPulseMVC.Services
{
    public partial class FitbitService
    {
        public string FitbitStepsFileName { get; set; }
        private string _csvPath;
        private string _tempFileStepsCSVPath;
        private string folderPath;

        public FitbitService()
        {
            FitbitStepsFileName = "StepsData.csv";
            _csvPath = $@"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/Fitbit/{FitbitStepsFileName}";
            _tempFileStepsCSVPath = $"temp_{FitbitStepsFileName}";
            folderPath = @"Nagendra/SelfCode/DatabaseInCSV/Fitbit";
        }
        public async Task<List<StepsData>> GetAndSaveLatestStepsData()
        {
            List<StepsData> lstStepsDataFinal = new List<StepsData>();
            DateTime startDate = new DateTime(2016, 05, 27);
            DateTime endDate = DateTime.Today.AddDays(-1); // Yesterday
            List<StepsData> lstStepsData = await GetStepsDataFromCsvUsingGraph();
            DateTime? maxSavedDate = GetMaxSavedDate(_tempFileStepsCSVPath, "Steps");

            if (maxSavedDate.HasValue)
            {
                startDate = maxSavedDate.Value.AddDays(1); // Start from the next day after the last saved date
            }

            while (startDate <= endDate)
            {
                DateTime fetchEndDate = startDate.AddYears(1).AddDays(-1); // Fetch up to one year at a time
                if (fetchEndDate > endDate)
                {
                    fetchEndDate = endDate;
                }

                List<StepsData> stepsData = await (new FitbitApiClient()).FetchWeeklyStepsAsync(startDate, fetchEndDate);
                AppendStepsDataToCsv(_tempFileStepsCSVPath, stepsData);

                await UploadDataToGraphAsync();
                startDate = fetchEndDate.AddDays(1); // Move to the next day after the fetched range
                await Task.Delay(1000); // Wait for 1 second before the next fetch
            }

            lstStepsDataFinal = await GetStepsDataFromCsvUsingGraph();
            if (File.Exists(_tempFileStepsCSVPath))
            {
                File.Delete(_tempFileStepsCSVPath);
            }
            return lstStepsDataFinal;
        }
        public void AppendStepsDataToCsv(string filePath, List<StepsData> stepsDataList)
        {
            bool fileExists = File.Exists(filePath);

            using (var writer = new StreamWriter(filePath, append: true))
            {
                if (!fileExists)
                {
                    // Write the header if the file does not exist
                    writer.WriteLine("FitbitType,Date,Steps");
                }

                foreach (var stepsData in stepsDataList)
                {
                    writer.WriteLine($"Steps,{stepsData.DateOfActivity.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)},{stepsData.StepsValue}");
                }
            }
        }
        public DateTime? GetMaxSavedDate(string filePath, string fitbitType)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            DateTime? maxDate = null;

            using (var reader = new StreamReader(filePath))
            {
                string headerLine = reader.ReadLine(); // Skip the header line

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var columns = line.Split(',');

                    if (columns.Length >= 2 && columns[0] == fitbitType)
                    {
                        if (DateTime.TryParseExact(columns[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            if (maxDate == null || date > maxDate)
                            {
                                maxDate = date;
                            }
                        }
                    }
                }
            }

            return maxDate;
        }

        public async Task<List<StepsData>> GetStepsDataFromCsvUsingGraph()
        {
            var stepsDataList = new List<StepsData>();
            try
            {
                await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, FitbitStepsFileName, _tempFileStepsCSVPath);
                if (!File.Exists(_tempFileStepsCSVPath))
                {
                    // If the file does not exist, create it with a header
                    using (var writer = new StreamWriter(_tempFileStepsCSVPath, false, Encoding.UTF8))
                    {
                        writer.WriteLine("FitbitType,Date,Steps");
                    }
                }
                var fileContents = await File.ReadAllLinesAsync(_tempFileStepsCSVPath, Encoding.UTF8);

                foreach (var line in fileContents.Skip(1)) // Skip the header line
                {
                    var columns = line.Split(',');
                    if (columns.Length >= 3 && columns[0] == "Steps")
                    {
                        if (DateTime.TryParseExact(columns[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                            int.TryParse(columns[2], out int steps))
                        {
                            stepsDataList.Add(new StepsData
                            {
                                DateOfActivity = date,
                                StepsValue = steps
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return stepsDataList;
        }


        public async Task UploadDataToGraphAsync()
        {
            try
            {
                await GraphFileUtility.UploadFile(folderPath, FitbitStepsFileName, _tempFileStepsCSVPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading data to Graph: {ex.Message}");
                throw ex;
            }
        }

    }
}