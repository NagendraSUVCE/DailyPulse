using System.Data;
using YahooFinanceApi;
public class StockNetworthService
{
    private List<StockPurchase> lstPurchases { get; set; }
    public StockNetworthService()
    {
        
    }
    public async Task<StockNetworth> GetLatestNetworthForGivenDate(string stockSymbol = "MSFT", DateTime? givenDate = null)
    {
        if (givenDate == null)
        {
            givenDate = DateTime.Now;
        }
        StockNetworth stockNetworth = new StockNetworth();
        stockNetworth.Symbol = stockSymbol;
        stockNetworth.NetworthGivendate = givenDate;



        DataSet dsNew = new DataSet(); decimal netWorthInINR = 0;
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols
        lstPurchases ??= await (new StockFileService()).GetStockPurchases();
        Candle latestCandle = await (new YahooFinanceService()).GetPriceGivenStockOnGivenDate(stockSymbol, givenDate.Value);
        Candle latestCandleINRUSDExchange = await (new YahooFinanceService()).GetPriceGivenStockOnGivenDate("INR=X", givenDate.Value);

        // Calculate total net worth
        decimal totalQuantity = lstPurchases
            .Where(purchase => purchase.PurchaseDate <= givenDate)
            .Sum(purchase => purchase.Quantity ?? 0);
        decimal netWorth = totalQuantity * latestCandle.AdjustedClose;
        stockNetworth.TotalQuantityOnGivenDate = totalQuantity;
        if (stockSymbol == "MSFT")
        {
            netWorthInINR = totalQuantity * latestCandle.AdjustedClose * latestCandleINRUSDExchange.AdjustedClose;
            stockNetworth.PriceOfStockOnGivenDate = latestCandle.AdjustedClose * latestCandleINRUSDExchange.AdjustedClose;
            stockNetworth.PriceOfStockOnGiveDateInDollars = latestCandle.AdjustedClose;
        }
        else
        {
            netWorthInINR = netWorth;
        }
        netWorth = Math.Round(netWorth, 2);
        netWorthInINR = Math.Round(netWorthInINR, 2);

        stockNetworth.TotalNetworthOnGivenDate = netWorthInINR;
        stockNetworth.TotalNetworthOnGivenDateInDollars = netWorth;
        return stockNetworth;
    }

    private async Task FillInAllRequiredDetails()
    {
        lstPurchases ??= await (new StockFileService()).GetStockPurchases();
    }
}