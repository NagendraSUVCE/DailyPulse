
using System.Data;
using Models.DailyLog;
using Models.Finance;

public class ExpensesService
{
    public async Task<List<ExpensesCustom>> GetAllExpenses()

    {
        // var filePath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/all Payslips/all Payslips Summarized.xlsx";
        // DataSet dataSet = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePath);
       DataSet dataSet = await GetPayslipsSummarizedGraphWay();
        List<ExpensesCustom> lstExpenses = new List<ExpensesCustom>();
        ExpensesCustom expensesObj = new ExpensesCustom();
        for (int i = 0; i < dataSet.Tables.Count; i++)
        {
            DataTable dtExpense = dataSet.Tables[i];
            if (dtExpense.TableName == "Expenses2022")
            {
                for (int j = 0; j < dtExpense.Rows.Count; j++)
                {
                    expensesObj = new ExpensesCustom();
                    try
                    {
                        expensesObj.TxnDate = Convert.ToDateTime(dtExpense.Rows[j][0]);
                        expensesObj.Daily15MinLogId = dtExpense.Rows[j][1].ToString();
                        expensesObj.ExpneseDescription = dtExpense.Rows[j][2].ToString();
                        expensesObj.ExpenseCategory = dtExpense.Rows[j][3].ToString();
                        expensesObj.ExpenseSubCategory = dtExpense.Rows[j][4].ToString();
                        expensesObj.ExpensePaymentType = dtExpense.Rows[j][5].ToString();
                        expensesObj.ExpenseVendor = dtExpense.Rows[j][6].ToString();
                        expensesObj.ExpenseAmt = Convert.ToDecimal(dtExpense.Rows[j][7]);

                        lstExpenses.Add(expensesObj);
                    }
                    catch (Exception ex)
                    {
                        expensesObj.ExpenseDeposits = 0;
                    }

                }
            }
        }
        return lstExpenses;
    }

    public async Task<List<ExpensesCustom>> GetAllExpensesByDate(DateTime date)
    {
        var allExpenses = await GetAllExpenses();
        var filteredExpenses = allExpenses.Where(e => e.TxnDate.Date == date.Date).ToList();
        return filteredExpenses;
    }


    public async Task<List<DailyLog15Min>> GetAllExpensesLogPending()
    {
        List<DailyLog15Min> lstDailyLog15MinExpensesPending = new List<DailyLog15Min>();
        var lstDaily15MinLog = (new Daily15MinLogService()).GetDaily15MinLogAsync().Result;
        lstDailyLog15MinExpensesPending = lstDaily15MinLog.Where(log => log.activityDesc != null && log.activityDesc.ToLower().EndsWith("rs")).ToList();
        // var allExpenses = await GetAllExpenses();
        // lstDailyLog15MinExpensesPending.RemoveAll(log => 
        //     allExpenses.Any(exp => 
        //         exp.Daily15MinLogId.Trim().ToLower() == log.activityDesc.Trim().ToLower() &&
        //         exp.TxnDate.Date == log.dtActivity.Date
        //     )
        // );


        ExpensesPending expensesPendingObj = new ExpensesPending();
        expensesPendingObj.ExpensesPendingLogRunDate = DateTime.Now;
        expensesPendingObj.InExpenseSheet = 0;
        expensesPendingObj.CompletelyAccounted = 0;
        expensesPendingObj.InDailyLogSheet = lstDaily15MinLog.Count(log => log.activityDesc != null && log.activityDesc.ToLower().EndsWith("rs"));
        expensesPendingObj.AccountingPending = 0;
        expensesPendingObj.ExpensesLoggingPending = 0;
        expensesPendingObj.ExpenseLoggingAutomatic = 0;
        expensesPendingObj.ExpenseMisc = "ToBeFilledVisualStudioCode";

        var filePathExpenses = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/all Payslips/all Payslips Summarized.xlsx";
        DataSet dataSetExpenses = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePathExpenses);
        DataTable dataTableExpenses = dataSetExpenses.Tables[0];
        for (int i = 0; i < dataSetExpenses.Tables.Count; i++)
        {
            DataTable dtExpense = dataSetExpenses.Tables[i];
            if (dtExpense.TableName == "Expenses2022")
            {

                expensesPendingObj.InExpenseSheet = dtExpense.Rows.Count;
                for (int j = 0; j < dtExpense.Rows.Count; j++)
                {
                    var activityDesc = dtExpense.Rows[j][1].ToString();
                    var expensesDesc = dtExpense.Rows[j][2].ToString();
                    if (string.IsNullOrEmpty(expensesDesc) ||
                        expensesDesc.Equals("ToBeFilledVisualStudioCode", StringComparison.OrdinalIgnoreCase) ||
                        expensesDesc.StartsWith("ToBefilled", StringComparison.OrdinalIgnoreCase))
                    {
                        expensesPendingObj.AccountingPending++;
                    }
                    else
                    {
                        expensesPendingObj.CompletelyAccounted++;
                    }

                    var matchingLog = lstDailyLog15MinExpensesPending
                         .FirstOrDefault(log => log.activityDesc.ToLower() == activityDesc.ToLower());
                    if (matchingLog != null)
                    {
                        lstDailyLog15MinExpensesPending.Remove(matchingLog);
                    }
                }
            }
        }
        List<ExpensesCustom> lstExpenses = new List<ExpensesCustom>();
        foreach (var log in lstDailyLog15MinExpensesPending)
        {

            ExpensesCustom expensesObj = new ExpensesCustom();
            expensesObj.TxnDate = log.dtActivity;
            expensesObj.Daily15MinLogId = log.activityDesc;
            expensesObj.ExpneseDescription = "ToBeFilledVisualStudioCode";
            Utility.Excel.ExcelUtilities.UpdateExpensesPendingObject(expensesObj);
            expensesPendingObj.ExpensesLoggingPending++;
            expensesPendingObj.ExpenseLoggingAutomatic++;
            lstExpenses.Add(expensesObj);
        }
        var filePathExpensesPending = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/ExpensesPending.csv";

        Utility.Excel.ExcelUtilities.CreateCsvFromList(lstExpenses, filePathExpensesPending);
        var filePathExpensesPendingSummary = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/ExpensesPendingSummary.csv";

        // Prepare the line to append
        var lineToAppend = $"{expensesPendingObj.ExpensesPendingLogRunDate},{expensesPendingObj.InExpenseSheet},{expensesPendingObj.CompletelyAccounted},{expensesPendingObj.InDailyLogSheet},{expensesPendingObj.AccountingPending},{expensesPendingObj.ExpensesLoggingPending},{expensesPendingObj.ExpenseLoggingAutomatic},{expensesPendingObj.ExpenseMisc}";

        // Check if the file exists
        if (!File.Exists(filePathExpensesPendingSummary))
        {
            // Create the file and write the header and the first line
            var header = "ExpensesPendingLogRunDate,InExpenseSheet,CompletelyAccounted,InDailyLogSheet,AccountingPending,ExpensesLoggingPending,ExpenseLoggingAutomatic,ExpenseMisc";
            File.WriteAllLines(filePathExpensesPendingSummary, new[] { header, lineToAppend });
        }
        else
        {
            // Append the line to the existing file
            File.AppendAllText(filePathExpensesPendingSummary, lineToAppend + Environment.NewLine);
        }

        return lstDailyLog15MinExpensesPending;
    }


        public async Task<System.Data.DataSet> GetPayslipsSummarizedGraphWay()
        {
            var tempFilePath = "payslips.xlsx";
            var fileName = "all Payslips Summarized.xlsx";
            var folderPath = @"Nagendra/all Salary/all Payslips";
            System.Data.DataSet ds = null;
            try
            {
                await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, tempFilePath);
                ds = GraphFileUtility.GetDataFromExcelNewWayUseHeader(tempFilePath);

            }
            catch (Exception ex)
            {
                throw;
            }
            return ds;
        }

}