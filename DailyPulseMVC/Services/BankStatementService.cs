using System.Data;
using System.Globalization;
using Models.DailyLog;
using Models.Finance;

public class BankStatementService
{
    private const int SlNo = 1;
    private const int TxnDateIndex = 3;
    private const int TxnRemarksIndex = 5;
    private const int WithdrawalIndex = 6;
    private const int DepositIndex = 7;
    private const int BalanceIndex = 8;
    public List<BankStmt> bankStmts = new List<BankStmt>();

    private BankStmt GetBankStmtGivenRow(DataRow bankStmtRow)
    {
        BankStmt bankStmtObj = new BankStmt();

        DateTime txnDate = DateTime.MinValue;
        if (DateTime.TryParseExact(bankStmtRow[TxnDateIndex].ToString(), "dd/MM/yyyy",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out txnDate))
        {
            bankStmtObj.BankName = "ICICI";
            bankStmtObj.TxnDate = txnDate;
            bankStmtObj.TxnDetails = bankStmtRow[TxnRemarksIndex].ToString();
            bankStmtObj.Misc = ""; //bankStmtRow[TxnRemarksIndex].ToString();
            decimal txnAmt = 0;
            if (decimal.TryParse(bankStmtRow[WithdrawalIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Withdrawals = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[DepositIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Deposits = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[BalanceIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Balance = txnAmt;
            }
        }
        return bankStmtObj;
    }
    private List<BankStmt> GetBankStmtsGivenDataTable(DataTable bankStmtTable)
    {
        var foundTxn = false;
        BankStmt bankStatementSingleRow = new BankStmt();
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        foreach (DataRow bankStmtRow in bankStmtTable.Rows)
        {
            string colValue = bankStmtRow[SlNo]?.ToString() ?? string.Empty;
            if (colValue.Contains("S No."))
            {
                foundTxn = true;
                continue;
            }
            if (foundTxn)
            {
                bankStatementSingleRow = GetBankStmtGivenRow(bankStmtRow);
                if (bankStatementSingleRow.TxnDate != null && bankStatementSingleRow.TxnDate != DateTime.MinValue)
                {
                    lstBankStmt.Add(bankStatementSingleRow);
                }
                else
                {
                    continue; // Skip processing if TxnDate is null or default
                }
            }
        }
        return lstBankStmt;
    }
    private List<BankStmt> GetStatementFromICICIGivenFilename(string fileName)
    {
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        DataSet ds = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(fileName);
        if (ds != null && ds.Tables.Count > 0)
        {
            DataTable dt = ds.Tables[0];
            lstBankStmt = GetBankStmtsGivenDataTable(dt);
        }
        return lstBankStmt;
    }
    public async Task<List<BankStmt>> GetBankDetailsICICI()
    {
        return await Task.Run(() =>
        {
            List<BankStmt> lstBankStatements = new List<BankStmt>();
            try
            {
                //string baseFolder = @"E:\OneDrive - Krishna\Nagendra\all Salary\Bank statements\ICICI\";
                var baseFolder = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/Bank statements/ICICI/";

                List<string> strLinks = new List<string>();
                ///*
                strLinks.Add(baseFolder + @"ICICI 2013 OpTransactionHistory2013.xls");
                strLinks.Add(baseFolder + @"ICICI 2014 OpTransactionHistory2014.xls");
                strLinks.Add(baseFolder + @"ICICI 2015 OpTransactionHistory2015.xls");
                strLinks.Add(baseFolder + @"ICICI 2016 OpTransactionHistory2016.xls");
                strLinks.Add(baseFolder + @"ICICI 2017 01 03 Jan To Mar OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI 2017 2018 OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI 2018 2019 OpTransactionHistory30-05-2019.xls");
                strLinks.Add(baseFolder + @"ICICI 2019 2020 OpTransactionHistory09-11-2020.xls");
                strLinks.Add(baseFolder + @"ICICI 2020 2021 OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI 2021 2022 OpTransactionHistoryTpr16-12-2022.xls");
                strLinks.Add(baseFolder + @"ICICI 2022 2023 OpTransactionHistoryTpr Downloaded 27 jan 2024.xls"); //*/
                strLinks.Add(baseFolder + @"ICICI 2023 2024 OpTransactionHistoryTpr04-01-2025 Full year.xls");
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(GetStatementFromICICIGivenFilename(strlink));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
            return lstBankStatements;
        });
    }

    private BankStmt GetBankStmtGivenRowAxis(DataRow bankStmtRow)
     {
        BankStmt bankStmtObj = new BankStmt();
        string input = bankStmtRow[0].ToString().Replace("\u202F", " ").Trim(); // Replace narrow no-break space with regular space

        DateTime txnDate = DateTime.MinValue;
        if (DateTime.TryParseExact(input, "dd/MM/yyyy hh:mm:ss tt",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out txnDate))
        {
            bankStmtObj.BankName = "AXIS";
            bankStmtObj.TxnDate = txnDate;
            bankStmtObj.TxnDetails = bankStmtRow[2].ToString();
            bankStmtObj.Misc = ""; //bankStmtRow[TxnRemarksIndex].ToString();
            decimal txnAmt = 0;
            if (decimal.TryParse(bankStmtRow[3].ToString(), out txnAmt))
            {
                bankStmtObj.Withdrawals = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[4].ToString(), out txnAmt))
            {
                bankStmtObj.Deposits = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[5].ToString(), out txnAmt))
            {
                bankStmtObj.Balance = txnAmt;
            }
        }
        return bankStmtObj;
    }
    private List<BankStmt> GetBankStmtsGivenDataTableAxis(DataTable bankStmtTable)
    {
        var foundTxn = false;
        BankStmt bankStatementSingleRow = new BankStmt();
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        foreach (DataRow bankStmtRow in bankStmtTable.Rows)
        {
            string colValue = bankStmtRow[0]?.ToString() ?? string.Empty;
            if (colValue.Contains("Tran Date"))
            {
                foundTxn = true;
                continue;
            }
            if (foundTxn)
            {
                bankStatementSingleRow = GetBankStmtGivenRowAxis(bankStmtRow);
                if (bankStatementSingleRow.TxnDate != null && bankStatementSingleRow.TxnDate != DateTime.MinValue)
                {
                    lstBankStmt.Add(bankStatementSingleRow);
                }
                else
                {
                    continue; // Skip processing if TxnDate is null or default
                }
            }
        }
        return lstBankStmt;
    }
 private List<BankStmt> GetStatementFromAxisGivenFilename(string fileName)
    {
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        DataSet ds = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(fileName);
        if (ds != null && ds.Tables.Count > 0)
        {
            DataTable dt = ds.Tables[0];
            lstBankStmt = GetBankStmtsGivenDataTableAxis(dt);
        }
        return lstBankStmt;
    }
    public async Task<List<BankStmt>> GetBankDetailsAxis()
    {
        return await Task.Run(() =>
        {
            List<BankStmt> lstBankStatements = new List<BankStmt>();
            try
            {
                var baseFolder = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/Bank statements/CITI AXIS/";

                List<string> strLinks = new List<string>();
                strLinks.Add(baseFolder + @"2022 2023 CITI AXIS July 22 March 23.xlsx");
                strLinks.Add(baseFolder + @"2023 2024 2023 Apr 2024 Mar CITI AXIS.xlsx");
                strLinks.Add(baseFolder + @"2024 2025 2024 Apr 2025 Mar CITI AXIS.xlsx");
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(GetStatementFromAxisGivenFilename(strlink));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
            return lstBankStatements;
        });
    }



    public async Task<DataTable> ReconcileBankStatementsWithExpenses()
    {
        var expenseService = new ExpensesService();
        var lstExpenses = await expenseService.GetAllExpenses();
        var lstBankStatement = await GetBankDetailsICICI();

        // Add logic to reconcile bank statements with expenses and return a DataTable
        DataTable reconciliationResult = new DataTable();
        // Populate reconciliationResult with appropriate data
        reconciliationResult.Columns.Add("TxnDate", typeof(DateTime));
        reconciliationResult.Columns.Add("TransactionRemarks", typeof(string));
        reconciliationResult.Columns.Add("BankWithdrawal", typeof(decimal));
        reconciliationResult.Columns.Add("ExpenseAmount", typeof(decimal));
        reconciliationResult.Columns.Add("Remarks", typeof(string));

        foreach (var bankStmt in lstBankStatement)
        {
            if (bankStmt.TxnDate == DateTime.MinValue || bankStmt.TxnDate == null || bankStmt.Withdrawals <= 0)
            {
                continue; // Skip processing if TxnDate is null or default
            }

            var matchingExpenses = lstExpenses
            .Where(expense => expense.TxnDate.Date == bankStmt.TxnDate && expense.ExpenseAmt == bankStmt.Withdrawals)
            .ToList();

            if (matchingExpenses.Any())
            {
                foreach (var expense in matchingExpenses)
                {
                    var row = reconciliationResult.NewRow();
                    row["TxnDate"] = bankStmt.TxnDate;
                    row["TransactionRemarks"] = bankStmt.Misc;
                    row["BankWithdrawal"] = bankStmt.Withdrawals;
                    row["ExpenseAmount"] = expense.ExpenseAmt;
                    row["Remarks"] = "Matched";
                    reconciliationResult.Rows.Add(row);
                }
            }
            else
            {
                // Check for upper ceiling match
                var upperCeilingMatch = lstExpenses
                .Where(expense => expense.TxnDate.Date == bankStmt.TxnDate && expense.ExpenseAmt == Math.Ceiling(bankStmt.Withdrawals))
                .ToList();

                if (upperCeilingMatch.Any())
                {
                    foreach (var expense in upperCeilingMatch)
                    {
                        var row = reconciliationResult.NewRow();
                        row["TxnDate"] = bankStmt.TxnDate;
                        row["TransactionRemarks"] = bankStmt.Misc;
                        row["BankWithdrawal"] = bankStmt.Withdrawals;
                        row["ExpenseAmount"] = expense.ExpenseAmt;
                        row["Remarks"] = "Matched with upper ceiling";
                        reconciliationResult.Rows.Add(row);
                    }
                }
                else
                {
                    var row = reconciliationResult.NewRow();
                    row["TxnDate"] = bankStmt.TxnDate;
                    row["TransactionRemarks"] = bankStmt.Misc;
                    row["BankWithdrawal"] = bankStmt.Withdrawals;
                    row["ExpenseAmount"] = DBNull.Value;
                    row["Remarks"] = "No matching expense found";
                    reconciliationResult.Rows.Add(row);
                }
            }
        }
        return reconciliationResult;
    }
}