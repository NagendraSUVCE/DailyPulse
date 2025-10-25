using System.Data;
using Models.DailyLog;
using Microsoft.Extensions.Configuration;
using DailyPulseMVC.Services;
public class Daily15MinLogService
{
    private DataSet dataSet = null;
    public Daily15MinLogService()
    {
        if (dataSet == null)
        {
            // InitializeDataSetAsync().Wait();
        }
    }

    private async Task InitializeDataSetAsync()
    {
        dataSet = await GetDaily15MinLogFromGraphExcel();
    }

    public async Task<System.Data.DataSet> GetDaily15MinLogFromGraphExcel()
    {
        var tempFilePath = "timesheetbyte.xlsx";
        var fileName = "15-Min-Timesheet-168-Hours v2.xlsx";
        var folderPath = "Nagendra/000 Frequent";
        System.Data.DataSet ds = null;
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        try
        {
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, tempFilePath);
            ds = GraphFileUtility.GetDataFromExcelNewWay(tempFilePath);
        }
        catch (Exception ex)
        {
            // https://learn.microsoft.com/en-us/answers/questions/1191723/problem-extract-data-using-microsoft-graph-c-net
            throw;
        }
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
        return ds;
    }
    public async Task<List<DailyLog15Min>> GetDaily15MinLogAsyncForYear2025()
    {
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        var temp = await GetDaily15MinLogAsync();

        lstDailyLog15Min = temp.Where(log => log.dtActivity.Year == 2025).ToList();
        return lstDailyLog15Min;
    }
    public async Task<List<DailyLog15Min>> GetDaily15MinLogAsync()
    {

        try
        {
            if (dataSet == null)
            {
                dataSet = await GetDaily15MinLogFromGraphExcel();
            }
            if (dataSet == null || dataSet.Tables.Count < 7)
            {
                throw new Exception("DataSet is null or does not contain enough tables.");
            }
        }
        catch (Exception ex)
        {
            var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/000 Frequent/15-Min-Timesheet-168-Hours v2.xlsx";
            dataSet = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePath);

        }
        //return dataSet;



        DateTime dtDateOfActivity = new DateTime(2000, 01, 01);
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();

        var temp = await GetAll15MinLogs(dataSet.Tables[0]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;
        // Removed unused variable 'sheetIndex'
        temp = await GetAll15MinLogs(dataSet.Tables[1]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;

        temp = await GetAll15MinLogs(dataSet.Tables[2]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;


        temp = await GetAll15MinLogs(dataSet.Tables[3]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;


        temp = await GetAll15MinLogs(dataSet.Tables[4]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;
        temp = await GetAll15MinLogs(dataSet.Tables[5]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;

        temp = await GetAll15MinLogs(dataSet.Tables[6]);
        lstDailyLog15Min.AddRange(temp.ToList());
        temp = null;

        return lstDailyLog15Min;
    }

    public async Task<List<DailyLog15Min>> GetAll15MinLogs(DataTable dt15minActivity)
    {
        DateTime dtDateOfActivity = new DateTime(2000, 01, 01);
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        for (int j = 0; j < dt15minActivity.Columns.Count; j++)
        {
            for (int i = 0; i < dt15minActivity.Rows.Count; i++)
            {
                object dataCell = dt15minActivity.Rows[i][j];
                DailyLog15Min dailyLog15MinObj = new DailyLog15Min();
                if (dataCell != null && !String.IsNullOrWhiteSpace(dataCell.ToString()))
                {
                    DateTime dtTempForParse;
                    if (DateTime.TryParse(dataCell.ToString(), out dtTempForParse))
                    {
                        if (dtTempForParse.Year > 2018)
                        {
                            dtDateOfActivity = dtTempForParse;
                            dtDateOfActivity = dtDateOfActivity.AddHours(5);
                        }

                    }
                    else
                    {
                        if (i > 2)
                        {
                            string activityDesc = dataCell.ToString();
                            dailyLog15MinObj.dtActivity = dtDateOfActivity;
                            dailyLog15MinObj.activityDesc = activityDesc;
                            dailyLog15MinObj.rowIndex = i;
                            dailyLog15MinObj.colIndex = j;
                            // dailyLog15MinObj.category = SetAllFieldsForDailyActivity(dailyLog15MinObj);
                            dailyLog15MinObj.Hrs = 0.25m;
                            if (!String.IsNullOrWhiteSpace(dailyLog15MinObj.activityDesc))
                            {
                                string[] splitDesc = dailyLog15MinObj.activityDesc.Split('-');
                                if (splitDesc != null && splitDesc.Count() > 0)
                                {

                                    dailyLog15MinObj.category = splitDesc[0];
                                }

                                if (splitDesc != null && splitDesc.Count() > 1)
                                {

                                    dailyLog15MinObj.activityGroup = splitDesc[1];
                                }

                                if (splitDesc != null && splitDesc.Count() > 2)
                                {

                                    dailyLog15MinObj.activityName = splitDesc[2];
                                }

                                if (splitDesc != null && splitDesc.Count() > 3)
                                {

                                    dailyLog15MinObj.activityIndex = splitDesc[3];
                                }
                            }
                            lstDailyLog15Min.Add(dailyLog15MinObj);
                            dtDateOfActivity = dtDateOfActivity.AddMinutes(15);
                        }
                    }
                }
            }
        }
        return lstDailyLog15Min;
    }

    public async Task<List<DailyLogSummaryForEachDay>> GetDailyLogSummaryForEachDaysAsync()
    {
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        var temp = await GetDaily15MinLogAsync();
        lstDailyLog15Min.AddRange(temp.ToList());
        var categories = new[] { "SelfHelp", "SelfCode", "SelfTech", "SelfSong", "FitbitDailySteps" };
        List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay = GetSummaryForEachDay(lstDailyLog15Min, categories);
        var fitbitService = new FitbitService();
        List<StepsData> lstStepsData = await fitbitService.GetAndSaveLatestStepsData();

        foreach (var stepData in lstStepsData)
        {
            var matchingEntry = lstDailyLogSummaryForEachDay
            .FirstOrDefault(summary => summary.ActivityDate.HasValue && summary.ActivityDate.Value.Date == stepData.DateOfActivity && summary.Category == "FitbitDailySteps");

            if (matchingEntry != null)
            {
                matchingEntry.TotalValue = stepData.StepsValue;
            }
            else
            {
                lstDailyLogSummaryForEachDay.Add(new DailyLogSummaryForEachDay
                {
                    ActivityDate = stepData.DateOfActivity,
                    Category = "FitbitDailySteps",
                    TotalValue = stepData.StepsValue
                });
            }
        }
        return lstDailyLogSummaryForEachDay;
    }
    public async Task<DataSet> AvgStreak()
    {
        var categories = new[] { "SelfHelp", "SelfCode", "SelfTech", "SelfSong", "FitbitDailySteps" };
        List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay = await GetDailyLogSummaryForEachDaysAsync();
        SaveDailyLogSummaryForEachDayToCSV(lstDailyLogSummaryForEachDay);
        // Group data by year and category
        var groupedData = lstDailyLogSummaryForEachDay
            .Where(log => categories.Contains(log.Category))
            .GroupBy(log => new { Year = log.ActivityDate?.Year, log.Category })
            .Select(g => new
            {
                Year = g.Key.Year,
                Category = g.Key.Category,
                TotalValue = g.Sum(log => log.TotalValue),
                DaysCount = g.Select(log => log.ActivityDate?.Date).Distinct().Count()
            })
            .ToList();

        // Get distinct years and categories
        var years = groupedData.Select(g => g.Year).Distinct().OrderBy(y => y).ToList();

        // Create a DataTable for total hours
        var totalHoursTable = new DataTable("TotalValue");
        totalHoursTable.Columns.Add("Category");
        foreach (var year in years)
        {
            totalHoursTable.Columns.Add(year.ToString(), typeof(decimal));
        }

        // Populate the total hours DataTable
        foreach (var category in categories)
        {
            var row = totalHoursTable.NewRow();
            row["Category"] = category;
            foreach (var year in years)
            {
                var totalHrs = groupedData
                    .Where(g => g.Year == year && g.Category == category)
                    .Select(g => g.TotalValue)
                    .FirstOrDefault();
                row[year.ToString()] = totalHrs;
            }
            totalHoursTable.Rows.Add(row);
        }

        // Create a DataTable for average hours
        var averageHoursTable = new DataTable("AverageHours");
        averageHoursTable.Columns.Add("Category");
        foreach (var year in years)
        {
            averageHoursTable.Columns.Add(year.ToString(), typeof(decimal));
        }

        // Populate the average hours DataTable
        foreach (var category in categories)
        {
            var row = averageHoursTable.NewRow();
            row["Category"] = category;
            foreach (var year in years)
            {
                var dataForYearAndCategory = groupedData
                    .Where(g => g.Year == year && g.Category == category)
                    .FirstOrDefault();

                var totalHrs = dataForYearAndCategory?.TotalValue ?? 0;
                var daysCount = dataForYearAndCategory?.DaysCount ?? 0;

                // Assume 365 days for the year if no entries exist
                var daysInYear = (year == DateTime.Now.Year) ? DateTime.Now.DayOfYear : 365;
                var averageHrs = daysCount > 0 ? totalHrs / daysInYear : 0;
                row[year.ToString()] = averageHrs;
            }
            averageHoursTable.Rows.Add(row);
        }
        DataTable past30DaysTable = Past14DaysData(lstDailyLogSummaryForEachDay, categories);
        var last14DaysData = lstDailyLogSummaryForEachDay
            .Where(log => log.ActivityDate?.Date >= DateTime.Now.Date.AddDays(-15) && log.ActivityDate?.Date < DateTime.Now.Date)
            .ToList();
        DataTable weeklyData = GetDynamicWeeklySummary(last14DaysData, DateTime.Now.AddDays(-14), DateTime.Now);
        // Convert the summary list to a DataTable
        DataTable categoriesTotalHrsEachDay = DataTableConverter.ToDataTable(lstDailyLogSummaryForEachDay);

        var targets = new Dictionary<string, decimal>
{
    { "SelfCode", 0.25m },
            { "SelfTech", 0.34m },
    { "SelfHelp", 0.50m },
    { "SelfSong", 0.25m }, { "FitbitDailySteps", 11500m }
};



        List<StreakResult> streakResults = StreakAnalyzer.AnalyzeStreaks(lstDailyLogSummaryForEachDay, targets);



        // Ensure the class is defined or imported
        // Uncomment and replace the following line with the correct implementation if needed:
        // Daily15MinLogFileService daily15MinLogFileServiceObj = new Daily15MinLogFileService();
        // DataTable streakResultsDatatable = await daily15MinLogFileServiceObj.SaveStreakResultToFile(streakResults);

        // Add both tables to the DataSet
        var dsNew = new DataSet();
        // Add the past 30 days table to the DataSet
        dsNew.Tables.Add(past30DaysTable);

        // Add iteration tracking
        var iterationStartDate = new DateTime(2025, 09, 22); // Start of the current iteration
        var iterationLength = 14; // Length of iteration in days
        var today = DateTime.Now.Date;

        // Calculate the current iteration day
        var daysSinceStart = (today - iterationStartDate).Days;
        var currentIterationDay = (daysSinceStart % iterationLength) + 1;
        var dayRanges = new List<int> { currentIterationDay, 7, 30, 90, 365 };

        DataTable summaryTableForDailyRanges = new LogSummaryService().GetCategorySummary(lstDailyLogSummaryForEachDay, dayRanges, targets);

        dsNew.Tables.Add(summaryTableForDailyRanges);
        dsNew.Tables.Add(await ValidationsDataTable());
        dsNew.Tables.Add((new LogSummaryService().GetCategoryWeeklySummary(lstDailyLogSummaryForEachDay)));
        dsNew.Tables.Add(averageHoursTable);
        dsNew.Tables.Add(totalHoursTable);
        dsNew.Tables.Add(weeklyData);
        dsNew.Tables.Add(DataTableConverter.ToDataTable(streakResults));
        dsNew.Tables.Add(GetMonthWiseAverageForEachCategory(lstDailyLogSummaryForEachDay));

        // dsNew.Tables.Add(categoriesTotalHrsEachDay); // This renders too much data page becomes slow. Do not uncomment
        return dsNew;
    }

    private static DataTable Past14DaysData(List<DailyLogSummaryForEachDay> lstDailyLog15Min, string[] categories)
    {
        // Create a DataTable for the past 30 days
        var past30DaysTable = new DataTable("Past30Days");
        past30DaysTable.Columns.Add("Category");
        var past30Days = Enumerable.Range(0, 14)
                   .Select(offset => DateTime.Now.Date.AddDays(-offset))
                   .OrderBy(date => date)
                   .ToList();

        foreach (var date in past30Days)
        {
            past30DaysTable.Columns.Add(date.ToString("yyyy-MM-dd"), typeof(string));
        }

        // Populate the past 30 days DataTable
        foreach (var category in categories)
        {
            var row = past30DaysTable.NewRow();
            row["Category"] = category;
            foreach (var date in past30Days)
            {
                var totalHrsForDate = lstDailyLog15Min
                    .Where(log => log.ActivityDate?.Date == date && log.Category == category)
                    .Sum(log => log.TotalValue);

                // Use HTML for tick and cross marks
                if (totalHrsForDate >= 10000 && category == "FitbitDailySteps")
                {
                    row[date.ToString("yyyy-MM-dd")] = "<span style='color:green;'>&#x2705;</span>"; // Green tick
                }
                else if (totalHrsForDate > 0 && category != "FitbitDailySteps")
                {
                    row[date.ToString("yyyy-MM-dd")] = "<span style='color:green;'>&#x2705;</span>"; // Green tick
                }
                else
                {
                    row[date.ToString("yyyy-MM-dd")] = "<span style='color:red;'>&#10008;</span>"; // Red cross
                }
            }
            past30DaysTable.Rows.Add(row);
        }

        return past30DaysTable;
    }

    public static DataTable GetDynamicWeeklySummary(
    List<DailyLogSummaryForEachDay> logs,
    DateTime startDate,
    DateTime endDate)
    {
        // Normalize start to previous Sunday
        int daysToSunday = ((int)startDate.DayOfWeek + 7 - (int)DayOfWeek.Sunday) % 7;
        DateTime normalizedStart = startDate.AddDays(-daysToSunday).Date;

        // Normalize end to next Saturday
        int daysToSaturday = ((int)DayOfWeek.Saturday - (int)endDate.DayOfWeek + 7) % 7;
        DateTime normalizedEnd = endDate.AddDays(daysToSaturday).Date;

        // Prepare DataTable
        var table = new DataTable();
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("WeekStartDate", typeof(DateTime));
        string[] dayColumns = { "01-Sun", "02-Mon", "03-Tue", "04-Wed", "05-Thu", "06-Fri", "07-Sat" };
        foreach (var col in dayColumns)
            table.Columns.Add(col, typeof(string));

        // Group logs by Category
        var categories = logs.Select(l => l.Category).Distinct();

        foreach (var category in categories)
        {
            DateTime weekStart = normalizedStart;

            while (weekStart <= normalizedEnd)
            {
                DateTime weekEnd = weekStart.AddDays(6);

                var weekLogs = logs
                    .Where(l => l.Category == category && l.ActivityDate?.Date >= weekStart && l.ActivityDate?.Date <= weekEnd)
                    .ToList();

                if (weekLogs.Any())
                {
                    var row = table.NewRow();
                    row["Category"] = category;
                    row["WeekStartDate"] = weekStart;

                    foreach (var log in weekLogs)
                    {
                        int dayIndex = (int)log.ActivityDate?.DayOfWeek;
                        string columnName = dayColumns[dayIndex];
                        row[columnName] = (row[columnName] == DBNull.Value ? 0 : (decimal)row[columnName]) + log.TotalValue;
                        decimal decValue = 0.25m;
                        if (decimal.TryParse(row[columnName]?.ToString(), out decValue))
                        //if (row[columnName] != DBNull.Value && row[columnName] is decimal value)
                        {
                            if (category == "FitbitDailySteps")
                            {
                                row[columnName] = decValue >= 10000
                                    ? "<span style='color:green;'>&#x2705;</span>" // Green tick
                                    : "<span style='color:red;'>&#10008;</span>"; // Red cross
                            }
                            else
                            {
                                row[columnName] = decValue > 0
                                    ? "<span style='color:green;'>&#x2705;</span>" // Green tick
                                    : "<span style='color:red;'>&#10008;</span>"; // Red cross
                            }
                        }

                    }

                    table.Rows.Add(row);
                }

                weekStart = weekStart.AddDays(7); // Move to next week
            }
        }

        // Sort rows by WeekStartDate descending
        var sortedRows = table.AsEnumerable()
            .OrderBy(r => r.Field<string>("Category"))
            .ThenByDescending(r => r.Field<DateTime>("WeekStartDate"))
            .ToList();

        var sortedTable = table.Clone(); // Clone structure
        foreach (var row in sortedRows)
            sortedTable.ImportRow(row);

        return sortedTable;
    }



    private static List<DailyLogSummaryForEachDay> GetSummaryForEachDay(List<DailyLog15Min> lstDailyLog15Min, string[] categories)
    {
        List<DailyLogSummaryForEachDay> summaryList = new List<DailyLogSummaryForEachDay>();

        // Group data by date and category for faster lookup
        var groupedData = lstDailyLog15Min
            .GroupBy(log => new { log.dtActivity.Date, log.category })
            .Select(g => new
            {
                Date = g.Key.Date,
                Category = g.Key.category,
                TotalHrs = g.Sum(log => log.Hrs)
            })
            .ToDictionary(g => new { g.Date, g.Category }, g => g.TotalHrs);

        // Get the date range from the data
        var startDate = lstDailyLog15Min.Min(log => log.dtActivity.Date);
        var endDate = lstDailyLog15Min.Max(log => log.dtActivity.Date);

        // Generate all dates in the range
        var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        // Populate the summary list
        foreach (var date in allDates)
        {
            foreach (var category in categories)
            {
                var key = new { Date = date, Category = category };
                var totalHrs = groupedData.TryGetValue(key, out var hrs) ? hrs : 0;

                summaryList.Add(new DailyLogSummaryForEachDay
                {
                    ActivityDate = date,
                    Category = category,
                    TotalValue = totalHrs
                });
            }
        }

        return summaryList;
    }

    private async Task<DataTable> ValidationsDataTable()
    {

        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        var temp = await GetDaily15MinLogAsync();
        lstDailyLog15Min.AddRange(temp.ToList());

        // Convert the summary list to a DataTable
        DataTable validationsTable = DataTableConverter.ToDataTable(GetValidations(lstDailyLog15Min));
        return validationsTable;
    }
    private static List<Validations> GetValidations(List<DailyLog15Min> lstDailyLog15Min)
    {
        List<Validations> validations = new List<Validations>();
        var groupedByDate = lstDailyLog15Min
            .GroupBy(log => log.dtActivity.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() });

        foreach (var group in groupedByDate)
        {
            if (group.Count != 96)
            {
                validations.Add(new Validations
                {
                    ValidationMessage = $"Validation failed for date {group.Date:yyyy-MM-dd}: Expected 96 entries, but found {group.Count}."
                });
            }
        }

        return validations;
    }

    private static void SaveDailyLogSummaryForEachDayToCSV(List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay)
    {
        /* DataTable dataTable = DataTableConverter.ToDataTable(lstDailyLogSummaryForEachDay);
         string baseFolder = "wwwroot\\files\\Daily15MinLog";
         string fileName = "DailyLogSummaryForEachDay.csv";
         bool overwriteAllContents = true;
         DataTableConverter.DataTableToCsv(dataTable, baseFolder, fileName, overwriteAllContents);
         */
    }

    private DataTable GetMonthWiseAverageForEachCategory(List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay)
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Year", typeof(string));
        dataTable.Columns.Add("Category", typeof(string));
        for (int month = 1; month <= 12; month++)
        {
            dataTable.Columns.Add(new DateTime(1, month, 1).ToString("MMM"), typeof(decimal));
        }

        var groupedByYearAndCategory = lstDailyLogSummaryForEachDay
            .GroupBy(log => new { Year = log.ActivityDate?.Year, log.Category })
            .OrderByDescending(g => g.Key.Year)
            .ThenBy(g => g.Key.Category);

        foreach (var group in groupedByYearAndCategory)
        {
            var row = dataTable.NewRow();
            row["Year"] = "Year-" + group.Key.Year;
            row["Category"] = group.Key.Category;

            for (int month = 1; month <= 12; month++)
            {
                var monthlyLogs = group
                    .Where(log => log.ActivityDate?.Month == month)
                    .ToList();

                var totalHrs = monthlyLogs.Sum(log => log.TotalValue);
                var daysCount = monthlyLogs.Select(log => log.ActivityDate?.Date).Distinct().Count();

                // Average based on the number of entries in that particular month
                var averageHrs = daysCount > 0 ? totalHrs / daysCount : 0;
                row[new DateTime(1, month, 1).ToString("MMM")] = averageHrs > 0 ? averageHrs : DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}