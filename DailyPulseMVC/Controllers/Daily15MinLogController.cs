using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.DailyLog;

namespace DailyPulseMVC.Controllers;

public class Daily15MinLogController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<Daily15MinLogController> _logger;

    public Daily15MinLogController(ILogger<Daily15MinLogController> logger)
    {
        _logger = logger;
    }


    public async Task<IActionResult> Index()
    {
        var lstDailyLog15Min = await (new Daily15MinLogService()).GetDaily15MinLogAsyncForYear2025();
        return View(lstDailyLog15Min);
    }

    public async Task<IActionResult> Common()
    {
       DataSet dataSet = (new ExpensesService()).GetPayslipsSummarizedGraphWay().Result;
        DataTable dataTable1 = dataSet.Tables[0];
        foreach (DataTable table in dataSet.Tables)
        {
            if (table.TableName == "Expenses2022")
            {
                dataTable1 = table;
                break;
            }
        }
        DataSet dsNew = new DataSet();
        dsNew.Tables.Add(dataTable1.Copy());
        return await Task.FromResult(View(dsNew));
    }

    public IActionResult AvgStreak()
    {
        DataSet dsNew = new DataSet();
        dsNew = (new Daily15MinLogService()).AvgStreak().Result;

        return View("Common", dsNew);
    }

    public IActionResult Reconciliation()
    {
        DataSet dsNew = new DataSet();
        DataTable dt = (new BankStatementService()).ReconcileBankStatementsWithExpenses().Result;
        dsNew.Tables.Add(dt);

        return View("Common", dsNew);
    }

    public IActionResult Charts()
    {
       List<DailyLogSummaryForEachDay> lstDailyLogSummary = (new Daily15MinLogService()).GetDailyLogSummaryForEachDaysAsync().Result;

        return View("Charts", lstDailyLogSummary);
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
