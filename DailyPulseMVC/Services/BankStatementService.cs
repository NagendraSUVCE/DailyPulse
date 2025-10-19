using System.Data;
using System.Globalization;
using Models.DailyLog;
using Models.Finance;
using System.Threading.Tasks;

public class BankConfig
{
    public string BankName { get; set; }
    public string FilePath { get; set; }
    public string DateFormat { get; set; }
    public string ColValueStartDesc { get; set; }
    public int ColValueStartColIndex { get; set; }
    public int TxnDateIndex { get; set; }
    public int TxnRemarksIndex { get; set; }
    public int WithdrawalIndex { get; set; }
    public int DepositIndex { get; set; }
    public int BalanceIndex { get; set; }
    public BankConfig(string bankName, string filePath, string colValueStartDesc, int colValueStartColIndex, string dateFormat, int txnDateIndex, int txnRemarksIndex, int withdrawalIndex, int depositIndex, int balanceIndex)
    {
        BankName = bankName;
        FilePath = filePath;
        ColValueStartDesc = colValueStartDesc;
        ColValueStartColIndex = colValueStartColIndex;
        DateFormat = dateFormat;
        TxnDateIndex = txnDateIndex;
        TxnRemarksIndex = txnRemarksIndex;
        WithdrawalIndex = withdrawalIndex;
        DepositIndex = depositIndex;
        BalanceIndex = balanceIndex;
    }

    public static List<BankConfig> GetBankConfigs()
    {
        return new List<BankConfig>
        {
            new BankConfig("ICICI", "/path/to/icici/file.xls", "S No.",1, "dd/MM/yyyy", 3, 5, 6, 7, 8),
            new BankConfig("AXIS", "/path/to/axis/file.xlsx", "Tran Date", 0, "dd/MM/yyyy hh:mm:ss tt", 0, 2, 3, 4, 5)
            // Add more bank configurations as needed
        };
    }
}
public class BankStatementService
{
    private const int TxnDateIndex = 3;
    private const int TxnRemarksIndex = 5;
    private const int WithdrawalIndex = 6;
    private const int DepositIndex = 7;
    private const int BalanceIndex = 8;
    public List<BankStmt> bankStmts = new List<BankStmt>();

    private BankStmt GetBankStatementGivenDataRow(DataRow bankStmtRow, BankConfig bankConfig)
    {
        BankStmt bankStmtObj = new BankStmt();
        string input = bankStmtRow[bankConfig.TxnDateIndex].ToString().Replace("\u202F", " ").Trim(); // Replace narrow no-break space with regular space

        DateTime txnDate = DateTime.MinValue;
        if (DateTime.TryParseExact(input, bankConfig.DateFormat,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out txnDate))
        {
            bankStmtObj.BankName = bankConfig.BankName;
            bankStmtObj.TxnDate = txnDate;
            bankStmtObj.TxnDetails = bankStmtRow[bankConfig.TxnRemarksIndex].ToString();
            bankStmtObj.Misc = ""; //bankStmtRow[TxnRemarksIndex].ToString();
            decimal txnAmt = 0;
            if (decimal.TryParse(bankStmtRow[bankConfig.WithdrawalIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Withdrawals = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[bankConfig.DepositIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Deposits = txnAmt;
            }
            if (decimal.TryParse(bankStmtRow[bankConfig.BalanceIndex].ToString(), out txnAmt))
            {
                bankStmtObj.Balance = txnAmt;
            }
        }
        return bankStmtObj;
    }
    private List<BankStmt> GetBankStatementsGivenDataTable(DataTable bankStmtTable, BankConfig bankConfig)
    {
        var foundTxn = false;
        BankStmt bankStatementSingleRow = new BankStmt();
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        foreach (DataRow bankStmtRow in bankStmtTable.Rows)
        {
            string colValue = bankStmtRow[bankConfig.ColValueStartColIndex]?.ToString() ?? string.Empty;
            if (colValue.Contains(bankConfig.ColValueStartDesc))
            {
                foundTxn = true;
                continue;
            }
            if (foundTxn)
            {
                bankStatementSingleRow = GetBankStatementGivenDataRow(bankStmtRow
                , bankConfig);
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
    private async Task<List<BankStmt>> GetBankStatementsGivenFilename(string fileName, BankConfig bankConfig)
    {
        var parts = fileName.Split('/');
        var fileNameOnly = parts[^1];
        var oneLevelUpFolder = parts[^2];
        var folderPath = "Nagendra/all Salary/Bank statements/";
        DataSet dsTemp = await GetBankStatementUsingGraph(folderPath, oneLevelUpFolder, fileNameOnly);
        List<BankStmt> lstBankStmt = new List<BankStmt>();
        // DataSet ds = Utility.Excel.ExcelUtilities.GetDataFromExcelNewWay(fileName);
        if (dsTemp != null && dsTemp.Tables.Count > 0)
        {
            DataTable dt = dsTemp.Tables[0];
            lstBankStmt = GetBankStatementsGivenDataTable(dt,
            bankConfig
            );
        }
        return lstBankStmt;
    }
    

    private async Task<System.Data.DataSet> GetBankStatementUsingGraph(string baseFolder, string oneLevelUpFolder, string fileNameOnly)
    {
        var tempFilePath = $"temp_{fileNameOnly}";
        string folderPath = baseFolder + oneLevelUpFolder;
        System.Data.DataSet ds = null;
        List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
        try
        {
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileNameOnly, tempFilePath);
            ds = GraphFileUtility.GetDataFromExcelNewWay(tempFilePath);
        }
        catch (Exception ex)
        {
            // https://learn.microsoft.com/en-us/answers/questions/1191723/problem-extract-data-using-microsoft-graph-c-net
            throw;
        }
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
        return ds;
    }
    public async Task<List<BankStmt>> GetBankStatementsForAllBanks()
    {
        return await Task.Run(async () =>
        {
            List<BankStmt> lstBankStatements = new List<BankStmt>();
            try
            {
                //string baseFolder = @"E:\OneDrive - Krishna\Nagendra\all Salary\Bank statements\ICICI\";
                var baseFolder = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/Bank statements/";

                List<string> strLinks = new List<string>();
                /*
                strLinks.Add(baseFolder + @"ICICI/ICICI 2013 OpTransactionHistory2013.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2014 OpTransactionHistory2014.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2015 OpTransactionHistory2015.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2016 OpTransactionHistory2016.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2017 01 03 Jan To Mar OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2017 2018 OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2018 2019 OpTransactionHistory30-05-2019.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2019 2020 OpTransactionHistory09-11-2020.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2020 2021 OpTransactionHistory14-06-2021.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2021 2022 OpTransactionHistoryTpr16-12-2022.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2022 2023 OpTransactionHistoryTpr Downloaded 27 jan 2024.xls");
                strLinks.Add(baseFolder + @"ICICI/ICICI 2023 2024 OpTransactionHistoryTpr04-01-2025 Full year.xls");*/
                strLinks.Add(baseFolder + @"ICICI/ICICI 2025 2026 OpTransactionHistory19-10-2025.xls");
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(await GetBankStatementsGivenFilename(strlink
                    , new BankConfig("ICICI", "", "S No.", 1, "dd/MM/yyyy", TxnDateIndex, TxnRemarksIndex, WithdrawalIndex,
                     DepositIndex, BalanceIndex)));
                }

                baseFolder = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/Bank statements/";
                strLinks.Clear();
                /* strLinks.Add(baseFolder + @"CITI AXIS/2022 2023 CITI AXIS July 22 March 23.xlsx");
                strLinks.Add(baseFolder + @"CITI AXIS/2023 2024 2023 Apr 2024 Mar CITI AXIS.xlsx");
                strLinks.Add(baseFolder + @"CITI AXIS/2024 2025 2024 Apr 2025 Mar CITI AXIS.xlsx"); */
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(await GetBankStatementsGivenFilename(strlink
                    , new BankConfig("AXIS", "", "Tran Date", 0, "dd/MM/yyyy hh:mm:ss tt", 0, 2, 3, 4, 5)));
                }

                baseFolder = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/all Salary/Bank statements/";
                strLinks.Clear();
                /*strLinks.Add(baseFolder + @"CITI/CITI 2010.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2011.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2012.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2013.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2014.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2015.txt");
                strLinks.Add(baseFolder + @"CITI/2016 Till march.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2016 2017.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2017 2018.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2018 2019.txt");
                strLinks.Add(baseFolder + @"CITI/CITI 2019 2020.txt");*/
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(GetBankDetailsCitiBankGivenFileName(strlink).Result);
                }

                strLinks.Clear();
                /* strLinks.Add(baseFolder + @"HDFC/Acct_Statement_XXXXXXXX4998_11thOct2025_2024_2025.xls"); */
                strLinks.Add(baseFolder + @"HDFC/Acct_Statement_XXXXXXXX4998_11thOct2025_2025_2026.xls");
                foreach (var strlink in strLinks)
                {
                    lstBankStatements.AddRange(await GetBankStatementsGivenFilename(strlink
                    , new BankConfig("HDFC", "", "Date", 0, "dd/MM/yy", 0, 1, 4, 5, 6)));
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



    public async Task<List<BankStmt>> GetBankDetailsCitiBankGivenFileName(string filePath)
    {
        string fulltext = string.Empty;
        List<BankStmt> lstBankStatements = new List<BankStmt>();
        return await Task.Run(async () =>
        {
            try
            {
                
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);
                    BankStmt currentBankStmt = null;

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.Length >= 10 &&
                        DateTime.TryParseExact(trimmedLine.Substring(0, 10), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None
                        , out DateTime txnDate))
                        {
                            // If the line starts with a date, create a new BankStmt object
                            currentBankStmt = new BankStmt
                            {
                                BankName = "CITI",
                                TxnDate = txnDate,
                                Misc = trimmedLine
                            };
                            lstBankStatements.Add(currentBankStmt);
                        }
                        else if (currentBankStmt != null)
                        {
                            // If the line does not start with a date and does not start with "Page ", "Date Transaction", "Closing Balance", or "Final Tally", append to the Misc field of the current BankStmt
                            if (!trimmedLine.StartsWith("Page ") &&
                                !trimmedLine.StartsWith("Date Transaction") &&
                                !trimmedLine.StartsWith("Closing Balance") &&
                                !trimmedLine.StartsWith("Final Tally") &&
                                trimmedLine.Length > 5)
                            {
                                currentBankStmt.Misc += " " + trimmedLine;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
            decimal previousBalance = 0;

            foreach (var bankStmt in lstBankStatements)
            {
                try
                {
                    var parts = bankStmt.Misc.Split(' ');
                    if (parts.Length > 1)
                    {
                        var secondLastPart = parts[^2].Trim();
                        var lastPart = parts.Last().Trim();

                        if (decimal.TryParse(lastPart, out decimal balance))
                        {
                            bankStmt.Balance = balance;

                            if (decimal.TryParse(secondLastPart, out decimal txnAmount))
                            {
                                if (balance > previousBalance)
                                {
                                    bankStmt.Deposits = txnAmount;
                                }
                                else
                                {
                                    bankStmt.Withdrawals = txnAmount;
                                }
                            }
                            else
                            {
                                bankStmt.TxnDetails = "Error: Unable to convert second last part to decimal";
                            }
                        }
                        else
                        {
                            bankStmt.TxnDetails = "Error: Unable to convert last part to decimal";
                        }

                        previousBalance = balance; // Update previous balance for next iteration
                    }
                    else
                    {
                        bankStmt.TxnDetails = "Error: Misc field does not contain enough parts";
                    }

                    // Populate TxnDetails with Misc but remove TxnDate, Withdrawals, Deposits, and Balance
                    var txnDetailsParts = parts.Skip(1).Take(parts.Length - 3).ToList(); // Remove last two parts
                    bankStmt.TxnDetails = string.Join(" ", txnDetailsParts);
                    bankStmt.TxnDetails = bankStmt.TxnDetails.Trim();
                    bankStmt.Misc = string.Empty;

                   // await ProcessBankStatements(bankStmt);
                }
                catch (Exception ex)
                {
                    bankStmt.TxnDetails = $"Error: {ex.Message}";
                }
            }
            await (new ProcessBankStatements()).ProcessBankStatementsRuleBased(lstBankStatements);
            return lstBankStatements;
        });
    }

   

  

    public async Task ProcessBankStatements(BankStmt bankStmt)
    {

     if (bankStmt.TxnDetails.StartsWith("CORPORATE CREDIT", StringComparison.OrdinalIgnoreCase))
        {
            bankStmt.TxnType = "Income";
            bankStmt.TxnCategory = "Salary";
            if (bankStmt.TxnDetails.ToLower().Contains("mindtree", StringComparison.OrdinalIgnoreCase))
            {
                bankStmt.TxnRemarks = "MINDTREE SALARY";
            }
        }
        else if (
            bankStmt.TxnDetails.StartsWith("FUND TRANSFER ", StringComparison.OrdinalIgnoreCase)
        && (bankStmt.TxnDetails.Contains("NEW INDIA ASSURANCE", StringComparison.OrdinalIgnoreCase)))
        {
            bankStmt.TxnType = "Income";
            bankStmt.TxnCategory = "Reimbursement";
            bankStmt.TxnRemarks = "MICROSOFT Insurance";
        }
        else if (bankStmt.TxnDetails.StartsWith("INTEREST EARNED", StringComparison.OrdinalIgnoreCase))
        {
            bankStmt.TxnType = "Income";
            bankStmt.TxnCategory = "Interest";
        }
        else if (bankStmt.TxnDetails.StartsWith("DIVIDEND CREDIT ", StringComparison.OrdinalIgnoreCase))
        {
            bankStmt.TxnType = "Income";
            bankStmt.TxnCategory = "Dividend";
        }
        else if (
            (bankStmt.TxnDetails.StartsWith("EFT TO ", StringComparison.OrdinalIgnoreCase)
            && (bankStmt.TxnDetails.Contains("016901606246", StringComparison.OrdinalIgnoreCase)
        )))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "Self";
            bankStmt.TxnRemarks = "To Nagendra ICICI";
        }
        else if (
            (bankStmt.TxnDetails.StartsWith("EFT TO ", StringComparison.OrdinalIgnoreCase)
            && (bankStmt.TxnDetails.Contains("01901610024998", StringComparison.OrdinalIgnoreCase)
        )))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "Rajani";
            bankStmt.TxnRemarks = "To Rajani HDFC";
        }
        else if (
            (bankStmt.TxnDetails.StartsWith("EFT TO ", StringComparison.OrdinalIgnoreCase)
            && (bankStmt.TxnDetails.Contains("0032500100680601", StringComparison.OrdinalIgnoreCase)
        )))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "Rajani";
            bankStmt.TxnRemarks = "To Rajani Karantaka Bank";
        }
        else if (bankStmt.TxnDetails.Contains("30914294783"))
        {
            bankStmt.TxnType = "Expense";
            bankStmt.TxnCategory = "Yoga";
            bankStmt.TxnRemarks = "To Santosh";
        }
        else if (bankStmt.TxnDetails.Contains("03171510000061"))
        {
            bankStmt.TxnType = "Expense";
            bankStmt.TxnCategory = "Rent";
            bankStmt.TxnRemarks = "Miyapur House Ravi Perala";
        }
        else if (bankStmt.TxnDetails.Contains("3327101002297"))
        {
            bankStmt.TxnType = "Expense";
            bankStmt.TxnCategory = "Rent";
            bankStmt.TxnRemarks = "Chandanagar House Vasantha Kumari";
        }
        else if (bankStmt.TxnDetails.ToLower().Contains("atm withdrawal")
        || bankStmt.TxnDetails.ToLower().Contains("atm cash withdrawal"))
        {
            bankStmt.TxnType = "Expense";
            bankStmt.TxnCategory = "ATM Withdrawal";
        }
        else if (bankStmt.TxnDetails.ToLower().Contains("payment for credit card"))
        {
            bankStmt.TxnType = "Expense";
            bankStmt.TxnCategory = "CreditCardPayment";
            bankStmt.TxnRemarks = "Credit Card Payment";
        }
        else if (bankStmt.TxnDetails.Contains("30XX9XXX52"))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "RecurringDeposit";
            bankStmt.TxnRemarks = "Natesh Central Bank";
        }
        else if (bankStmt.TxnDetails.Contains("Cheque No"))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "Transfer";
            bankStmt.TxnRemarks = " Central Bank";
        }
        else if (bankStmt.TxnDetails.Contains("PURCHASE SUBJECT"))
        {
            bankStmt.TxnType = "Expenses";
            bankStmt.TxnCategory = "Purchases";
            bankStmt.TxnRemarks = "Various";
        }
        else if (bankStmt.TxnDetails.Contains("Credited To"))
        {
            bankStmt.TxnType = "Expenses";
            bankStmt.TxnCategory = "ToOthers";
            bankStmt.TxnRemarks = "Various";
        }
        else if (bankStmt.TxnDetails.Contains("MOBILE RECHARGE"))
        {
            bankStmt.TxnType = "Expenses";
            bankStmt.TxnCategory = "Bills";
            bankStmt.TxnRemarks = "Mobile";
        }
        else if (
            (bankStmt.TxnDetails.StartsWith("IMPS OUTWARD ", StringComparison.OrdinalIgnoreCase)
            && (bankStmt.TxnDetails.Contains("TRANSFER TO SELF", StringComparison.OrdinalIgnoreCase)
        )))
        {
            bankStmt.TxnType = "Transfer";
            bankStmt.TxnCategory = "Self";
            bankStmt.TxnRemarks = "To Nagendra ICICI";
        }
        else
        {
            bankStmt.TxnType = "ToBeFilled";
            bankStmt.TxnCategory = "ToBeFilled";
        }
    }

    public async Task<List<Reconciliation>> ReconcileBankStatementsWithExpenses()
    {
        var expenseService = new ExpensesService();
        List<ExpensesCustom> lstExpenses = await expenseService.GetAllExpenses();
        var lstBankStatement = await GetBankStatementsForAllBanks();

        // Add logic to reconcile bank statements with expenses and return a DataTable
        List<Reconciliation> lstReconciliation = new List<Reconciliation>();
        Reconciliation reconciliationObj = new Reconciliation();

        foreach (var bankStmt in lstBankStatement)
        {
            if (bankStmt.TxnDate == DateTime.MinValue || bankStmt.TxnDate == null || bankStmt.Withdrawals <= 0)
            {
                continue; // Skip processing if TxnDate is null or default
            }

            var matchingExpenses = lstExpenses
            .Where(expense => expense.TxnDate.Date == bankStmt.TxnDate && expense.ExpenseAmt == bankStmt.Withdrawals)
            .ToList();
            reconciliationObj.BankName = bankStmt.BankName;
            if (matchingExpenses.Any())
            {
                foreach (var expense in matchingExpenses)
                {
                    reconciliationObj  = new Reconciliation();
                    reconciliationObj.TxnDate = bankStmt.TxnDate;
                    reconciliationObj.TransactionRemarks = bankStmt.TxnDetails;
                    reconciliationObj.BankWithdrawal = bankStmt.Withdrawals;
                    reconciliationObj.Daily15MinLogId = expense.Daily15MinLogId;
                    reconciliationObj.ExpenseAmount = expense.ExpenseAmt;
                    reconciliationObj.Remarks = "Matched";
                    lstReconciliation.Add(reconciliationObj);
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
                    reconciliationObj  = new Reconciliation();
                        reconciliationObj.TxnDate = bankStmt.TxnDate;
                        reconciliationObj.TransactionRemarks = bankStmt.TxnDetails;
                        reconciliationObj.BankWithdrawal = bankStmt.Withdrawals;
                        reconciliationObj.Daily15MinLogId = expense.Daily15MinLogId;
                        reconciliationObj.ExpenseAmount = expense.ExpenseAmt;
                        reconciliationObj.Remarks = "Matched with upper ceiling";
                        lstReconciliation.Add(reconciliationObj);
                    }
                }
                else
                {
                    reconciliationObj  = new Reconciliation();
                    reconciliationObj.TxnDate = bankStmt.TxnDate;
                    reconciliationObj.TransactionRemarks = bankStmt.TxnDetails;
                    reconciliationObj.BankWithdrawal = bankStmt.Withdrawals;
                    reconciliationObj.Daily15MinLogId = string.Empty;
                    reconciliationObj.Remarks = "No matching expense found";
                        lstReconciliation.Add(reconciliationObj);
                }
            }
        }
        return lstReconciliation;
    }
}