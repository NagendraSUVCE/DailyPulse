using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using DailyPulseMVC.Models;

namespace DailyPulseMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    
    public async Task<IActionResult> Index()
    {
        var lstDailyLog15Min = (new Daily15MinLogService()).GetDaily15MinLogAsyncForYear2025().Result;
        return View(lstDailyLog15Min);
    }

  public async Task<IActionResult> Common()
    {
        var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/all Payslips/all Payslips Summarized.xlsx";
        DataSet dataSet = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePath);
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

    public async Task<IActionResult> ExpensesPending()
    {
        var lstDailyLog15MinExpensesPending = (new ExpensesService()).GetAllExpensesLogPending().Result;
        return View("Index", lstDailyLog15MinExpensesPending);
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
