using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.Finance;

namespace DailyPulseMVC.Controllers;

public class ExpensesController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<ExpensesController> _logger;
    private BankStatementService _bankStatementService;

    public ExpensesController(ILogger<ExpensesController> logger)
    {
        _logger = logger;
        _bankStatementService = new BankStatementService();
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
        DataSet lstDailyLog15MinExpensesPending = await (new ExpensesService()).GetAllExpensesLogPending();
        return View("Common", lstDailyLog15MinExpensesPending);
    }


    public async Task<IActionResult> ExpensesPendingGraph()
    {
        var lstDailyLog15MinExpensesPending = await (new ExpensesService()).GetPayslipsSummarizedGraphWay();
        return View("Common", lstDailyLog15MinExpensesPending);
    }

    public async Task<IActionResult> Reconciliation()
    {
        DataSet dsNew = new DataSet();
        DataTable dt = await _bankStatementService.ReconcileBankStatementsWithExpenses();
        dsNew.Tables.Add(dt);

        return View("Common", dsNew);
    }

    public async Task<IActionResult> BankStatements()
    {
        DataSet dsNew = new DataSet();
        List<BankStmt> lstBankStmts = await _bankStatementService.GetBankDetailsICICI();
       
        lstBankStmts = lstBankStmts.Where(stmt => stmt.BankName == "CITI").ToList();
        lstBankStmts = lstBankStmts.Where(stmt => stmt.TxnType == "ToBeFilled").ToList();
        DataTable dataTablePurchases = DataTableConverter.ToDataTable(lstBankStmts);
        DataTable summaryTable = new DataTable();
        summaryTable.Columns.Add("NameOfTable", typeof(string));
        summaryTable.Columns.Add("CurrentDateTime", typeof(DateTime));
        summaryTable.Columns.Add("NumberOfRows", typeof(int));

        DataRow summaryRow = summaryTable.NewRow();
        summaryRow["NameOfTable"] = dataTablePurchases.TableName;
        summaryRow["CurrentDateTime"] = DateTime.Now;
        summaryRow["NumberOfRows"] = lstBankStmts.Count;

        summaryTable.Rows.Add(summaryRow);
        dsNew.Tables.Add(summaryTable);
        dsNew.Tables.Add(dataTablePurchases);
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
