using System.Data;
using Models.DailyLog;
using Microsoft.Extensions.Configuration;
public class Daily15MinLogService
{
    public Daily15MinLogService()
    {
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
            return ds;
        }
    public async Task<List<DailyLog15Min>> GetDaily15MinLogAsyncForYear2025()
    {
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        var temp = await (new Daily15MinLogService()).GetDaily15MinLogAsync();

        lstDailyLog15Min = temp.Where(log => log.dtActivity.Year == 2025).ToList();
        return lstDailyLog15Min;
    }
    public async Task<List<DailyLog15Min>> GetDaily15MinLogAsync()
    {
        // var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/000 Frequent/15-Min-Timesheet-168-Hours v2.xlsx";
        // DataSet dataSet = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePath);
       
        DataSet dataSet = await GetDaily15MinLogFromGraphExcel();
        if (dataSet == null || dataSet.Tables.Count < 7)
        {
            throw new Exception("DataSet is null or does not contain enough tables.");
        }

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

    public async Task<DataSet> AvgStreak()
    {
        
        DataSet dataSet = await GetDaily15MinLogFromGraphExcel();
        if (dataSet == null || dataSet.Tables.Count < 7)
        {
            throw new Exception("DataSet is null or does not contain enough tables.");
        }
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        var temp = (new Daily15MinLogService()).GetDaily15MinLogAsync().Result;
        lstDailyLog15Min.AddRange(temp.ToList());

        // Group data by year and category
        var groupedData = lstDailyLog15Min
            .Where(log => new[] { "SelfHelp", "SelfCode", "SelfTech", "SelfSong" }.Contains(log.category))
            .GroupBy(log => new { Year = log.dtActivity.Year, log.category })
            .Select(g => new
            {
                Year = g.Key.Year,
                Category = g.Key.category,
                TotalHrs = g.Sum(log => log.Hrs),
                DaysCount = g.Select(log => log.dtActivity.Date).Distinct().Count()
            })
            .ToList();

        // Get distinct years and categories
        var years = groupedData.Select(g => g.Year).Distinct().OrderBy(y => y).ToList();
        var categories = new[] { "SelfHelp", "SelfCode", "SelfTech", "SelfSong" };

        // Create a DataTable for total hours
        var totalHoursTable = new DataTable("TotalHours");
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
                    .Select(g => g.TotalHrs)
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

                var totalHrs = dataForYearAndCategory?.TotalHrs ?? 0;
                var daysCount = dataForYearAndCategory?.DaysCount ?? 0;

                // Assume 365 days for the year if no entries exist
                var daysInYear = (year == DateTime.Now.Year) ? DateTime.Now.DayOfYear : 365;
                var averageHrs = daysCount > 0 ? totalHrs / daysInYear : 0;
                row[year.ToString()] = averageHrs;
            }
            averageHoursTable.Rows.Add(row);
        }
        DataTable past30DaysTable = Past14DaysData(lstDailyLog15Min, categories);
        List<DailyLogSummaryForEachDay> lstDailyLogSummaryForEachDay = GetSummaryForEachDay(lstDailyLog15Min, categories);
        // Convert the summary list to a DataTable
        DataTable categoriesTotalHrsEachDay = DataTableConverter.ToDataTable(lstDailyLogSummaryForEachDay);

        var targets = new Dictionary<string, decimal>
{
    { "SelfCode", 0.25m },
            { "SelfTech", 0.34m },
    { "SelfHelp", 0.50m },
    { "SelfSong", 0.25m }
};

        List<StreakResult> streakResults = StreakAnalyzer.AnalyzeStreaks(lstDailyLogSummaryForEachDay, targets);

        foreach (var result in streakResults)
        {
            Console.WriteLine($"You met continuous target of {targets[result.Category]} for {result.StreakDays} days " +
                              $"starting from {result.EndDate:dd MMM yyyy} till {result.StartDate:dd MMM yyyy} " +
                              $"and you need {result.RequiredToday:F2} today to increase streak to {result.StreakDays + 1} days.");
        }

        DataTable streakResultsDatatable = DataTableConverter.ToDataTable(streakResults);
        DataTableConverter.DataTableToCsv(streakResultsDatatable
        , @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/"
        , "StreakResults.csv", false);
        // Add both tables to the DataSet
        var dsNew = new DataSet();
        // Add the past 30 days table to the DataSet
        dsNew.Tables.Add(past30DaysTable);
        dsNew.Tables.Add(streakResultsDatatable);
        dsNew.Tables.Add(averageHoursTable);
        dsNew.Tables.Add(totalHoursTable);
        // dsNew.Tables.Add(categoriesTotalHrsEachDay);


        return dsNew;
    }

    private static DataTable Past14DaysData(List<DailyLog15Min> lstDailyLog15Min, string[] categories)
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
                    .Where(log => log.dtActivity.Date == date && log.category == category)
                    .Sum(log => log.Hrs);

                // Use HTML for tick and cross marks
                if (totalHrsForDate > 0)
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
                    TotalHrs = totalHrs
                });
            }
        }

        return summaryList;
    }
}