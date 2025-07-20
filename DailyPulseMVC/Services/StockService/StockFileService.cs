
using System.Data;
using System.Threading.Tasks;

public class StockFileService
{
    public async Task<List<StockPurchase>> GetStockPurchases()
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
                        Symbol = "MSFT",
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
        return stockPurchases;
    }

    public async Task<DataSet> GetMSFTPurchasesFromPayslipSummarized()
    {
        DataSet dataSet = await (new ExpensesService()).GetPayslipsSummarizedGraphWay();
        DataTable dataTable1 = dataSet.Tables[0];
        foreach (DataTable table in dataSet.Tables)
        {
            if (table.TableName == "MSFTSharesSummary")
            {
                dataTable1 = table;
                break;
            }
        }
        DataSet dsNew = new DataSet();
        dsNew.Tables.Add(dataTable1.Copy());
        return dsNew;
    }
}