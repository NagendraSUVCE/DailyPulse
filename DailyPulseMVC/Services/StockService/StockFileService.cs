
using System.Data;
using System.Threading.Tasks;

public class StockFileService
{
    private DataSet payslipSummarizedDataSet = null;
    private DataTable _ppfDataTable = null;
    private DataTable _epfDataTable = null;
    public async Task<List<StockPurchase>> GetAllPurchases()
    {
        List<StockPurchase> stockPurchases = new List<StockPurchase>();
        // stockPurchases.AddRange(await GetStockPurchasesMSFT());
        // stockPurchases.AddRange(await GetStockPurchasesMutualFunds());
        // stockPurchases.AddRange(await GetPPF());
        stockPurchases.AddRange(await GetEPFO());
        return stockPurchases;
    }
    // Removed redundant method to avoid ambiguity
    public async Task<List<StockPurchase>> GetStockPurchasesMSFT()
    {
        DataSet dsMSFTShares = await GetMSFTPurchasesFromPayslipSummarized();
        List<StockPurchase> stockPurchases = new List<StockPurchase>();
        StockPurchase stockPurchase = null;
        foreach (DataRow row in dsMSFTShares.Tables[0].Rows)
            try
            {
                var purchaseDate = row["TxnDate"] != DBNull.Value ? Convert.ToDateTime(row["TxnDate"]) : DateTime.MinValue;
                if (purchaseDate != DateTime.MinValue)
                {
                    stockPurchase = new StockPurchase
                    {
                        StockSymbol = "MSFT",
                        StockName = "Microsoft",
                        Quantity = row["NetShares"] != DBNull.Value ? Convert.ToDecimal(row["NetShares"]) : 0,
                        PurchasePrice = row["FairMarketValue"] != DBNull.Value ? Convert.ToDecimal(row["FairMarketValue"]) : 0,
                        PurchaseDate = purchaseDate
                    };
                    stockPurchases.Add(stockPurchase);
                }
            }
            catch (InvalidCastException ex)
            {
                // Log the error and continue processing other rows
                Console.WriteLine($"Invalid data format in row: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An error occurred while processing row: {ex.Message}");
            }
        foreach (var sp in stockPurchases)
        {
            // Assuming Candle is a placeholder for a valid type, replace it with the correct type or define it.
            var latestCandleINRUSDExchange = await (new YahooFinanceService()).GetPriceGivenStockOnGivenDate("INR=X", sp.PurchaseDate.Value);
            sp.ExchangeRate = latestCandleINRUSDExchange.Close;
            sp.TotalPurchaseValue = sp.Quantity * sp.PurchasePrice * sp.ExchangeRate;
        }
        return stockPurchases;
    }
    public async Task<List<StockPurchase>> GetStockPurchasesMutualFunds()
    {
        DataSet dsMutualFunds = await GetMutualFundsDetail();
        List<StockPurchase> stockPurchases = new List<StockPurchase>();
        StockPurchase stockPurchase = null;
        foreach (DataRow row in dsMutualFunds.Tables[0].Rows)
        {
            try
            {
                DateTime purchaseDate;
                if (DateTime.TryParseExact(row["TRADE_DATE"].ToString(), "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out purchaseDate))
                {
                    // Successfully parsed purchaseDate
                }
                else
                {
                    purchaseDate = DateTime.MinValue;
                }
                if (purchaseDate != DateTime.MinValue)
                {
                    stockPurchase = new StockPurchase
                    {
                        StockSymbol = row["PRODUCT_CODE"]?.ToString() ?? "",
                        YahooStockId = GetYahooStockId(row["PRODUCT_CODE"]?.ToString() ?? ""),

                        StockName = row["SCHEME_NAME"]?.ToString() ?? "",
                        Quantity = row["UNITS"] != DBNull.Value ? Convert.ToDecimal(row["UNITS"]) : 0,
                        PurchasePrice = row["PRICE"] != DBNull.Value ? Convert.ToDecimal(row["PRICE"]) : 0,
                        TotalPurchaseValue = row["AMOUNT"] != DBNull.Value ? Convert.ToDecimal(row["AMOUNT"]) : 0,
                        PurchaseDate = purchaseDate
                    };
                    stockPurchases.Add(stockPurchase);
                }
            }
            catch (InvalidCastException ex)
            {
                // Log the error and continue processing other rows
                Console.WriteLine($"Invalid data format in row: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An error occurred while processing row: {ex.Message}");
            }
        }
        return stockPurchases;
    }

    public async Task<List<StockPurchase>> GetPPF()
    {
        DataSet dsMSFTShares = await GetMSFTPurchasesFromPayslipSummarized();
        List<StockPurchase> stockPurchases = new List<StockPurchase>();
        StockPurchase stockPurchase = null;
        foreach (DataRow row in _ppfDataTable.Rows)
            try
            {
                  DateTime purchaseDate;
                if (DateTime.TryParse(row["TxnDate"].ToString(), out purchaseDate))
                {
                    purchaseDate = purchaseDate.Date; // Extract only the date part, discarding the time
                }
                else
                {
                    purchaseDate = DateTime.MinValue;
                } 
                if (purchaseDate != DateTime.MinValue)
                {
                    stockPurchase = new StockPurchase
                    {
                        StockSymbol = "PPF",
                        StockName = "PPF",
                        Quantity = row["Investment"] != DBNull.Value ? Convert.ToDecimal(row["Investment"]) : 0,
                        PurchasePrice = 1,
                        PurchaseDate = purchaseDate
                    };

                    if (stockPurchase.Quantity > 0)
                    {
                        stockPurchase.TotalPurchaseValue = stockPurchase.Quantity;
                        stockPurchases.Add(stockPurchase);
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                // Log the error and continue processing other rows
                Console.WriteLine($"Invalid data format in row: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An error occurred while processing row: {ex.Message}");
            }
        return stockPurchases;
    }
    public async Task<List<StockPurchase>> GetEPFO()
    {
        DataSet dsMSFTShares = await GetMSFTPurchasesFromPayslipSummarized();
        List<StockPurchase> stockPurchases = new List<StockPurchase>();
        StockPurchase stockPurchase = null;
        decimal EmployeeShare =0, EmployerShare=0;
        string remarks = string.Empty;
        foreach (DataRow row in _epfDataTable.Rows)
            try
            {
                DateTime purchaseDate;
                if (DateTime.TryParse(row["TxnDate"].ToString(), out purchaseDate))
                {
                    purchaseDate = purchaseDate.Date; // Extract only the date part, discarding the time
                }
                else
                {
                    purchaseDate = DateTime.MinValue;
                }
                if (purchaseDate != DateTime.MinValue)
                {
                    EmployeeShare = row["EmployeeShare"] != DBNull.Value ? Convert.ToDecimal(row["EmployerShare"]) : 0;
                    EmployerShare = row["EmployerShare"] != DBNull.Value ? Convert.ToDecimal(row["EmployeeShare"]) : 0;
                    remarks = row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : string.Empty;
                    stockPurchase = new StockPurchase
                    {
                        StockSymbol = "EPFO",
                        StockName = "EPFO",
                        Quantity = EmployeeShare + EmployerShare,
                        PurchasePrice = 1,
                        PurchaseDate = purchaseDate
                    };

                    if (stockPurchase.Quantity > 0 && remarks.ToLowerInvariant().Contains("cont."))
                    {
                        stockPurchase.TotalPurchaseValue = stockPurchase.Quantity;
                        stockPurchases.Add(stockPurchase);
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                // Log the error and continue processing other rows
                Console.WriteLine($"Invalid data format in row: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An error occurred while processing row: {ex.Message}");
            }
        return stockPurchases;
    }
    private string GetYahooStockId(string stockSymbol)
    {
        var yahooStockId = stockSymbol;
        if (stockSymbol == "HGFOFT")
        {
            yahooStockId = "0P0000XW7I.BO"; // Replace with actual Yahoo Finance ID for the mutual fund
        }
        else if (stockSymbol == "HINNPT")
        {
            yahooStockId = "0P0000XW7T.BO";
        }
        else if (stockSymbol == "HLFGN")
        {
            yahooStockId = "0P00005V09.BO";
        }
        else if (stockSymbol == "HLFGTN")
        {
            yahooStockId = "0P0000XW89.BO";
        }
        else if (stockSymbol == "HMCOGT")
        {
            yahooStockId = "0P0000XW8F.BO";
        }
        else if (stockSymbol == "HPREG")
        {
            yahooStockId = "0P0001EI1B.BO";
        }
        else if (stockSymbol == "HPREGT")
        {
            yahooStockId = "0P0001EI18.BO";
        }
        else if (stockSymbol == "P1191")
        {
            yahooStockId = "0P0000GB48.BO";
        }
        else if (stockSymbol == "P1565")
        {
            yahooStockId = "0P0000ZKW6.BO";
        }
        else if (stockSymbol == "P8042")
        {
            yahooStockId = "0P0000XWAT.BO";
        }
        else if (stockSymbol == "P8176")
        {
            yahooStockId = "0P0000XWAB.BO";
        }
        else if (stockSymbol == "PDFG")
        {
            yahooStockId = "0P00005WZZ.BO";
        }
        else if (stockSymbol == "L103G")
        {
            yahooStockId = "0P00005WF0.BO";
        }
        else if (stockSymbol == "LD103G")
        {
            yahooStockId = "0P0000XVJQ.BO";
        }
        /*
        HBFG	HDFCBALANCED.BO
HGFOFT	0P0000XW7I.BO
HINNPT	0P0000XW7T.BO
HLFGN	0P00005V09.BO
HLFGTN	0P0000XW89.BO
HMCOGT	0P0000XW8F.BO
HPREG	0P0001EI1B.BO
HPREGT	0P0001EI18.BO
P1191	0P0000GB48.BO
P1565	0P0000ZKW6.BO
P8042	0P0000XWAT.BO
P8176	0P0000XWAB.BO
PDFG	0P00005WZZ.BO
L103G	0P00005WF0.BO
LD103G	0P0000XVJQ.BO
        */
        return yahooStockId;
    }

    public async Task<DataSet> GetMSFTPurchasesFromPayslipSummarized()
    {
        if (payslipSummarizedDataSet == null)
        {
            payslipSummarizedDataSet = await (new ExpensesService()).GetPayslipsSummarizedGraphWay();
        }
        DataTable dataTable1 = payslipSummarizedDataSet.Tables[0];
        foreach (DataTable table in payslipSummarizedDataSet.Tables)
        {
            if (table.TableName == "MSFTSharesSummary")
            {
                dataTable1 = table;
            }
            if (table.TableName == "EPF")
            {
                _epfDataTable = table;
            }
            if (table.TableName == "PPF")
            {
                _ppfDataTable = table;
            }
        }
        DataSet dsNew = new DataSet();
        dsNew.Tables.Add(dataTable1.Copy());
        return dsNew;
    }
    public async Task<DataSet> GetMutualFundsSummary()
    {
        var fileName = "CAMS_Summary.xlsx";
        var tempFilePath = $"temp_{fileName}";
        var folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
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
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
        return ds;
    }
    public async Task<DataSet> GetMutualFundsDetail()
    {
        var fileName = "CAMS_Detail.xlsx";
        var tempFilePath = $"temp_{fileName}";
        var folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
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
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
        return ds;
    }
}