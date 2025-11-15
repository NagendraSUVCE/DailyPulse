using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using YahooFinanceApi; // Added namespace for Candle type
using FileManagerOneDrive; // Assuming this namespace contains OnedriveFileManager
namespace DailyPulseMVC.Controllers;

public class HomeController : Controller
{
    // az logout
    // az login
    // az login --tenant "66c27865-2b61-409c-965a-b99d27699f72"   
    // dotnet publish -c Release -o ./bin/Publish
    // dotnet build /nologo /verbosity:q /property:WarningLevel=0 /clp:ErrorsOnly
    private readonly ILogger<HomeController> _logger;
    private readonly Daily15MinLogService _daily15MinLogService;
    private readonly OnedriveFileManager _onedriveFileManager;


    public HomeController(ILogger<HomeController> logger, Daily15MinLogService daily15MinLogService, OnedriveFileManager onedriveFileManager)
    {
        _logger = logger;
        _daily15MinLogService = daily15MinLogService;
        _onedriveFileManager = onedriveFileManager;
    }



    public async Task<IActionResult> Ping()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Status", typeof(string));
        dataTable.Rows.Add("Connected");

        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return View("Common", dataSet);
    }
    public async Task<IActionResult> Index()
    {
        /*
         var mailKitInitial = new MailKitClass(); // Removed as MailKitInitial is undefined
         await mailKitInitial.MailKitInitial(DateTime.Now.Date);
        //*/
        // Fetch Fitbit data

        var lstDailyLog15Min = await _daily15MinLogService.GetDaily15MinLogAsyncForYear2025();
        return View(lstDailyLog15Min);
    }

    public async Task<IActionResult> Daily15MinLogGroupBy()
    {

        var lstDailyLog15Min = await _daily15MinLogService.GetDaily15MinLogAsyncForYear2025();
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("StartDate", typeof(DateTime));
        dataTable.Columns.Add("EndDate", typeof(DateTime));
        dataTable.Columns.Add("ActivityDesc", typeof(string));
        dataTable.Columns.Add("TotalHours", typeof(double));

        var groupedData = lstDailyLog15Min
            .GroupBy(log => log.activityDesc)
            .Select(group => new
            {
                ActivityDesc = group.Key,
                StartDate = group.Min(log => log.dtActivity),
                EndDate = group.Max(log => log.dtActivity),
                TotalHours = group.Sum(log => log.Hrs)
            })
            .OrderByDescending(item => item.EndDate);

        foreach (var item in groupedData)
        {
            dataTable.Rows.Add(item.StartDate, item.EndDate, item.ActivityDesc, item.TotalHours);
        }
        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return View("Common", dataSet);
    }


    public async Task<IActionResult> Daily15MinLogThisDatePreviousYear()
    {

        var lstDailyLog15Min = await _daily15MinLogService.GetDaily15MinLogAsync();

        var today = DateTime.Now;
        DataSet dataSet = new DataSet();

        // Group logs by year
        var logsGroupedByYear = lstDailyLog15Min
            .GroupBy(log => log.dtActivity.Year)
            .OrderByDescending(group => group.Key);

        foreach (var yearGroup in logsGroupedByYear)
        {
            var logsForThisYear = yearGroup
            .Where(log => log.dtActivity.Month == today.Month && log.dtActivity.Day == today.Day)
            .OrderByDescending(log => log.dtActivity.Year)
            .ToList();

            DataTable dataTable = new DataTable($"Year_{yearGroup.Key}");
            dataTable.Columns.Add("StartDate", typeof(DateTime));
            dataTable.Columns.Add("EndDate", typeof(DateTime));
            dataTable.Columns.Add("ActivityDesc", typeof(string));
            dataTable.Columns.Add("TotalHours", typeof(double));

            var groupedData = logsForThisYear
            .GroupBy(log => log.activityDesc)
            .Select(group => new
            {
                ActivityDesc = group.Key,
                StartDate = group.Min(log => log.dtActivity),
                EndDate = group.Max(log => log.dtActivity),
                TotalHours = group.Sum(log => log.Hrs)
            })
            .OrderByDescending(item => item.EndDate);

            foreach (var item in groupedData)
            {
            dataTable.Rows.Add(item.StartDate, item.EndDate, item.ActivityDesc, item.TotalHours);
            }

            dataSet.Tables.Add(dataTable);
        }

        return View("Common", dataSet);
    }
    public async Task<IActionResult> Daily15MinLogThisDatePreviousYearGroupedBy6Hours()
    {

        var lstDailyLog15Min = await _daily15MinLogService.GetDaily15MinLogAsync();
        var today = DateTime.Now;
        DataSet dataSet = new DataSet();

        // Group logs by year
        var logsGroupedByYear = lstDailyLog15Min
            .GroupBy(log => log.dtActivity.Year)
            .OrderByDescending(group => group.Key);

        foreach (var yearGroup in logsGroupedByYear)
        {
            var logsForThisYear = yearGroup
            .Where(log => log.dtActivity.Month == today.Month && log.dtActivity.Day == today.Day)
            .ToList();

            DataTable dataTable = new DataTable($"Year_{yearGroup.Key}");
            dataTable.Columns.Add("ActivityDesc_0_6", typeof(string));
            dataTable.Columns.Add("TotalHours_0_6", typeof(double));
            dataTable.Columns.Add("ActivityDesc_6_12", typeof(string));
            dataTable.Columns.Add("TotalHours_6_12", typeof(double));
            dataTable.Columns.Add("ActivityDesc_12_18", typeof(string));
            dataTable.Columns.Add("TotalHours_12_18", typeof(double));
            dataTable.Columns.Add("ActivityDesc_18_24", typeof(string));
            dataTable.Columns.Add("TotalHours_18_24", typeof(double));

            // Group data by time slots
            var groupedData = logsForThisYear
            .GroupBy(log => new { log.activityDesc, TimeSlot = log.dtActivity.Hour / 6 })
            .Select(group => new
            {
                ActivityDesc = group.Key.activityDesc,
                TimeSlot = group.Key.TimeSlot,
                TotalHours = group.Sum(log => log.Hrs)
            })
            .GroupBy(item => item.TimeSlot)
            .ToDictionary(
                g => g.Key,
                g => g.Select(item => new { item.ActivityDesc, item.TotalHours }).ToList()
            );

            // Determine the maximum number of rows needed for any time slot
            int maxRows = groupedData.Values.Max(list => list.Count);

            for (int i = 0; i < maxRows; i++)
            {
            var row = dataTable.NewRow();

            for (int timeSlot = 0; timeSlot < 4; timeSlot++)
            {
                if (groupedData.ContainsKey(timeSlot) && i < groupedData[timeSlot].Count)
                {
                var activity = groupedData[timeSlot][i];
                row[$"ActivityDesc_{timeSlot * 6}_{(timeSlot + 1) * 6}"] = activity.ActivityDesc;
                row[$"TotalHours_{timeSlot * 6}_{(timeSlot + 1) * 6}"] = activity.TotalHours;
                }
                else
                {
                row[$"ActivityDesc_{timeSlot * 6}_{(timeSlot + 1) * 6}"] = string.Empty;
                row[$"TotalHours_{timeSlot * 6}_{(timeSlot + 1) * 6}"] = 0.0;
                }
            }

            dataTable.Rows.Add(row);
            }

            dataSet.Tables.Add(dataTable);
        }

        return View("Common", dataSet);
    }
    public async Task<IActionResult> FileDetails()
    {
        var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/allFilesOfNagendraKrishna25Jul2024_2_full_2.xlsx";
        DataSet dataSet = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePath);
        return View("Common", dataSet);
    }

    public async Task<IActionResult> OnedriveFileTotalSize()
    {
        var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/T//Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/T";
        DataTable dataTable = _onedriveFileManager.GetFolderSizes(filePath);
        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return View("Common", dataSet);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
