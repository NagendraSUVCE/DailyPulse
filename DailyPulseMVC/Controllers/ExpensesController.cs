using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;

namespace DailyPulseMVC.Controllers;

public class ExpensesController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ILogger<ExpensesController> logger)
    {
        _logger = logger;
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
