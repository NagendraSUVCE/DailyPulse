using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using YahooFinanceApi; // Added namespace for Candle type
using FileManagerOneDrive; // Assuming this namespace contains OnedriveFileManager
namespace DailyPulseMVC.Controllers;

public class HomeController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    // dotnet build /nologo /verbosity:q /property:WarningLevel=0 /clp:ErrorsOnly
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }


    public async Task<IActionResult> Index()
    {
        /*
         var mailKitInitial = new MailKitClass(); // Removed as MailKitInitial is undefined
         await mailKitInitial.MailKitInitial(DateTime.Now.Date);
        //*/
         // Fetch Fitbit data
        
        var lstDailyLog15Min = await (new Daily15MinLogService()).GetDaily15MinLogAsyncForYear2025();
        return View(lstDailyLog15Min);
    }

    public async Task<IActionResult> Daily15MinLogGroupBy()
    {
        
        var lstDailyLog15Min = await (new Daily15MinLogService()).GetDaily15MinLogAsyncForYear2025();
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

    public async Task<IActionResult> ExpensesPending()
    {
        var lstDailyLog15MinExpensesPending = await (new ExpensesService()).GetAllExpensesLogPending();
        return View("Index", lstDailyLog15MinExpensesPending);
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
        DataTable dataTable = (new OnedriveFileManager()).GetFolderSizes(filePath);
        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return View("Common", dataSet);
    }

    /*    public async Task<IActionResult> ExpensesPendingGraph()
                {
                    var lstDailyLog15MinExpensesPending = await (new ExpensesService()).GetPayslipsSummarizedGraphWay();
                    return View("Common", lstDailyLog15MinExpensesPending);
                }

                public IActionResult Reconciliation()
                {
                    DataSet dsNew = new DataSet();
                    DataTable dt = (new BankStatementService()).ReconcileBankStatementsWithExpenses().Result;
                    dsNew.Tables.Add(dt);

                    return View("Common", dsNew);
                }*/

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
