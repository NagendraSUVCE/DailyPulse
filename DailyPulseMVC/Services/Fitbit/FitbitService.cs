using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DailyPulseMVC.Services
{
    public partial class FitbitService
    {
        public async Task GetAndSaveLatestStepsData()
        {
            string filePath = "StepsData.csv";
            DateTime startDate = new DateTime(2016, 05, 27);
            DateTime endDate = DateTime.Today.AddDays(-1); // Yesterday
            DateTime? maxSavedDate = GetMaxSavedDate(filePath, "Steps");

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
                AppendStepsDataToCsv(filePath, stepsData);

                startDate = fetchEndDate.AddDays(1); // Move to the next day after the fetched range
                await Task.Delay(1000); // Wait for 1 second before the next fetch
            }
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
    }
}