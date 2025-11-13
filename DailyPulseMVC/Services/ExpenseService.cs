
using System.Data;
using System.Globalization;
using Models.DailyLog;
using Models.Finance;

public class ExpensesService
{
    public async Task<List<ExpensesCustom>> GetAllExpenses()

    {
        // var filePathExpensesPending = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/ExpensesPending.csv";
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


    public async Task<DataSet> GetAllExpensesLogPending()
    {
        List<DailyLog15Min> lstDailyLog15MinExpensesPending = new List<DailyLog15Min>();
        var lstDaily15MinLog = (new Daily15MinLogService()).GetDaily15MinLogAsync().Result;
        lstDailyLog15MinExpensesPending = lstDaily15MinLog.Where(log => log.activityDesc != null && log.activityDesc.ToLower().EndsWith("rs")).ToList();

        ExpensesPending expensesPendingObj = new ExpensesPending();
        expensesPendingObj.ExpensesPendingLogRunDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        expensesPendingObj.InExpenseSheet = 0;
        expensesPendingObj.CompletelyAccounted = 0;
        expensesPendingObj.InDailyLogSheet = lstDaily15MinLog.Count(log => log.activityDesc != null && log.activityDesc.ToLower().EndsWith("rs"));
        expensesPendingObj.AccountingPending = 0;
        expensesPendingObj.ExpensesLoggingPending = 0;
        expensesPendingObj.ExpenseLoggingAutomatic = 0;
        expensesPendingObj.ExpenseMisc = "";

        // var filePathExpenses = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/all Payslips/all Payslips Summarized.xlsx";
        // DataSet dataSetExpenses = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(filePathExpenses);
        DataSet dataSetExpenses = await GetPayslipsSummarizedGraphWay();
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
                        expensesDesc.Contains("ToBeFilledVisualStudioCode", StringComparison.OrdinalIgnoreCase) ||
                        expensesDesc.Contains("ToBefilled", StringComparison.OrdinalIgnoreCase))
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
            expensesObj.ExpneseDescription = "";
            UpdateExpensesPendingObject(expensesObj);
            expensesPendingObj.ExpensesLoggingPending++;
            expensesPendingObj.ExpenseLoggingAutomatic++;
            expensesPendingObj.AccountingPending++; // bug fix - since these are not in expenses sheet either
            lstExpenses.Add(expensesObj);
        }
        List<ExpensesPending> expensesPendings = new List<ExpensesPending> { expensesPendingObj };
        var folderPath = @"Nagendra/SelfCode/DatabaseInCSV";

        string filePathExpensesPending = "ExpensesPending.csv";
        string temp_filePathExpensesPending = "temp_ExpensesPending.csv";
        CreateCsvFromList(lstExpenses, temp_filePathExpensesPending);
        await GraphFileUtility.UploadFile(folderPath, filePathExpensesPending, temp_filePathExpensesPending);

        string expensesPendingSummaryCSVFilePath = "ExpensesPendingSummary.csv";
        string temp_expensesPendingSummaryCSVFilePath = "Temp_ExpensesPendingSummary.csv";
        await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, expensesPendingSummaryCSVFilePath, temp_expensesPendingSummaryCSVFilePath);
        AppendExpensesPendingDataToCsv(temp_expensesPendingSummaryCSVFilePath, expensesPendings);
        await GraphFileUtility.UploadFile(folderPath, expensesPendingSummaryCSVFilePath, temp_expensesPendingSummaryCSVFilePath);

        if (File.Exists(temp_filePathExpensesPending))
        {
            File.Delete(temp_filePathExpensesPending);
        }

        if (File.Exists(temp_expensesPendingSummaryCSVFilePath))
        {
            File.Delete(temp_expensesPendingSummaryCSVFilePath);
        }
         DataTable expensesPendingsDatatable = DataTableConverter.ToDataTable(expensesPendings);
         DataSet ds = new DataSet();
         ds.Tables.Add(expensesPendingsDatatable);
         DataTable expensesDatatable = DataTableConverter.ToDataTable(lstExpenses);
         ds.Tables.Add(expensesDatatable);
         DataTable expensesPendingDatatable = DataTableConverter.ToDataTable(lstDailyLog15MinExpensesPending);
         ds.Tables.Add(expensesPendingDatatable);
        return ds;
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

    private void UpdateExpensesPendingObject(ExpensesCustom expensesCustom)
    {
        if (expensesCustom != null)
        {
            if (expensesCustom.Daily15MinLogId.ToLower().Contains("-food-"))
                expensesCustom.ExpenseCategory = "Food";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-bills-"))
                expensesCustom.ExpenseCategory = "Bills";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-travel-"))
                expensesCustom.ExpenseCategory = "Travel";

            var subCategoryMappings = new Dictionary<string, string>
                {
                    { "-eatoutside-", "EatOutside" },
                    { "-grocery-", "Grocery" },
                    { "-local-", "Local" },
                    { "-entertainment-", "Entertainment" },
                    { "-intercity-", "InterCity" },
                    { "-rent-", "Rent" },
                    { "-medicines-", "Medicines" },
                    { "-electricity-", "Electricity" },
                    { "-internet-", "Internet" },
                    { "-water-", "Water" },
                    { "-mobile-", "Mobile" },
                    { "-gas-", "Gas" },
                    { "-fuel-", "Fuel" },
                    { "-storage-", "Storage" },
                    { "-clothes-", "Clothes" },
                    { "-other-", "Other" }
                };

            foreach (var mapping in subCategoryMappings)
            {
                if (expensesCustom.Daily15MinLogId.ToLower().Contains(mapping.Key))
                {
                    expensesCustom.ExpenseSubCategory = mapping.Value;
                    break;
                }
            }



            if (expensesCustom.Daily15MinLogId.ToLower().Contains("-paytmicici-"))
                expensesCustom.ExpensePaymentType = "PaytmICICI";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-paytm-"))
                expensesCustom.ExpensePaymentType = "Paytm";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-rajanigpay-"))
                expensesCustom.ExpensePaymentType = "RajaniGPAY";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-nagendraciti-"))
                expensesCustom.ExpensePaymentType = "NagendraCITI";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-metrocard-"))
                expensesCustom.ExpensePaymentType = "MetroCard";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-cash-"))
                expensesCustom.ExpensePaymentType = "Cash";

            if (expensesCustom.Daily15MinLogId.ToLower().Contains("-taazakitchen-"))
                expensesCustom.ExpenseVendor = "TaazaKitchen";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-coffeethindi-"))
                expensesCustom.ExpenseVendor = "CoffeeThindi";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-swiggy-"))
                expensesCustom.ExpenseVendor = "Swiggy";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("rapido"))
                expensesCustom.ExpenseVendor = "Rapido";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("nammayatri"))
                expensesCustom.ExpenseVendor = "NammaYatri";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("nammametro"))
                expensesCustom.ExpenseVendor = "NammaMetro";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("-auto-"))
                expensesCustom.ExpenseVendor = "Auto";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("akshayakalpa"))
                expensesCustom.ExpenseVendor = "AkshayaKalpa";
            else if (expensesCustom.Daily15MinLogId.ToLower().Contains("hydmetro"))
                expensesCustom.ExpenseVendor = "HydMetro";


            if (expensesCustom.Daily15MinLogId.ToLower().EndsWith("rs"))
            {
                var parts = expensesCustom.Daily15MinLogId.Split('-');
                var lastPart = parts.LastOrDefault();
                lastPart = lastPart?.Trim().ToLower();
                if (lastPart != null && lastPart.EndsWith("rs"))
                {
                    var amountString = lastPart.Substring(0, lastPart.Length - 2); // Remove "rs"
                    if (decimal.TryParse(amountString, out var amount))
                    {
                        expensesCustom.ExpenseAmt = amount;
                    }
                }
            }
        }
    }


    public void AppendExpensesPendingDataToCsv(string filePath, List<ExpensesPending> expensesPendingList)
    {
        bool fileExists = File.Exists(filePath);

        using (var writer = new StreamWriter(filePath, append: true))
        {
            if (!fileExists)
            {
                // Create the file and write the header and the first line
                var header = "ExpensesPendingLogRunDate,InExpenseSheet,CompletelyAccounted,InDailyLogSheet,AccountingPending,ExpensesLoggingPending,ExpenseLoggingAutomatic,ExpenseMisc";
                writer.WriteLine(header);
            }

            foreach (var expensesPendingObj in expensesPendingList)
            {   // Prepare the line to append
                var lineToAppend = $"{expensesPendingObj.ExpensesPendingLogRunDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{expensesPendingObj.InExpenseSheet},{expensesPendingObj.CompletelyAccounted},{expensesPendingObj.InDailyLogSheet},{expensesPendingObj.AccountingPending},{expensesPendingObj.ExpensesLoggingPending},{expensesPendingObj.ExpenseLoggingAutomatic},{expensesPendingObj.ExpenseMisc}";

                writer.WriteLine(lineToAppend);
            }
        }
    }

    private void CreateCsvFromList(List<ExpensesCustom> lstDailyLog15Min, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            // Add headers
            writer.WriteLine("Date,Id,Description");

            // Add data
            foreach (var log in lstDailyLog15Min)
            {
                var line = $"{log.TxnDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{log.Daily15MinLogId},{log.ExpneseDescription},{log.ExpenseCategory},{log.ExpenseSubCategory},{log.ExpensePaymentType},{log.ExpenseVendor},{log.ExpenseAmt}";
                writer.WriteLine(line);
            }
        }
    }

    /* 
    var filePathExpensesPendingSummary = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/ExpensesPendingSummary.csv";

    // Prepare the line to append
    var lineToAppend = $"{expensesPendingObj.ExpensesPendingLogRunDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{expensesPendingObj.InExpenseSheet},{expensesPendingObj.CompletelyAccounted},{expensesPendingObj.InDailyLogSheet},{expensesPendingObj.AccountingPending},{expensesPendingObj.ExpensesLoggingPending},{expensesPendingObj.ExpenseLoggingAutomatic},{expensesPendingObj.ExpenseMisc}";

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
    }*/
}