using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;

namespace DailyPulseMVC.Controllers;

public class HomeController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
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

    public async Task<IActionResult> ExpensesPending()
    {
        var lstDailyLog15MinExpensesPending = await (new ExpensesService()).GetAllExpensesLogPending();
        return View("Index", lstDailyLog15MinExpensesPending);
    }

    public async Task<IActionResult> ExpensesPendingGraph()
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
