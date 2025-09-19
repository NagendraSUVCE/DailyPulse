using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.DailyLog;
using YahooFinanceApi; // Add the namespace for Candle type

namespace DailyPulseMVC.Controllers;

public class StockPriceController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<Daily15MinLogController> _logger;

    public StockPriceController(ILogger<Daily15MinLogController> logger)
    {
        _logger = logger;
    }
    public async Task<String> Index()
    {

        // Fetch Fitbit data
        /*
        
        }*/

        string ping = "StockPriceController Index";
        _logger.LogInformation(ping);
        return ping;
    }

    public async Task<IActionResult> Common()
    {
        DataSet dsNew = new DataSet();
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols

        dsNew = await (new StockFileService()).GetMSFTPurchasesFromPayslipSummarized();
        return View("Common", dsNew);
    }
    public async Task<IActionResult> ListOfStockPurchases()
    {
        DataSet dsNew = new DataSet();
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols

        List<StockPurchase> lstPurchases = await (new StockFileService()).GetStockPurchasesMSFT();
        lstPurchases.AddRange(await (new StockFileService()).GetStockPurchasesMutualFunds());
        DataTable dataTablePurchases = DataTableConverter.ToDataTable(lstPurchases);
        var summarizedPurchases = lstPurchases
            .GroupBy(p => p.StockName)
            .Select(g => new
            {
                StockName = g.Key,
                TotalQuantity = g.Sum(p => p.Quantity),
                TotalPurchasePrice = g.Sum(p => p.TotalPurchaseValue),
                FirstPurchaseDate = g.Min(p => p.PurchaseDate),
                LastPurchaseDate = g.Max(p => p.PurchaseDate)
            })
            .ToList();

        DataTable dataTableSummary = DataTableConverter.ToDataTable(summarizedPurchases);

        // Calculate the sum of TotalPurchaseValue ignoring negative values
        decimal totalPurchaseValueSum = lstPurchases
            .Where(p => p.TotalPurchaseValue.HasValue && p.TotalPurchaseValue > 0)
            .Sum(p => p.TotalPurchaseValue ?? 0);

        // Create a new DataTable to hold the sum
        DataTable totalPurchaseValueTable = new DataTable("TotalPurchaseValueSummary");
        totalPurchaseValueTable.Columns.Add("TotalPurchaseValue", typeof(decimal));

        // Add a single row with the calculated sum
        DataRow row = totalPurchaseValueTable.NewRow();
        row["TotalPurchaseValue"] = totalPurchaseValueSum;
        totalPurchaseValueTable.Rows.Add(row);

        // Add the new table to the DataSet
        dsNew.Tables.Add(totalPurchaseValueTable);

        dsNew.Tables.Add(dataTableSummary);
        dsNew.Tables.Add(dataTablePurchases);
        return View("Common", dsNew);
    }
    public async Task<IActionResult> ListOfStockPurchasesMutualFunds()
    {
        DataSet dsNew = new DataSet();
        dsNew = await (new StockFileService()).GetMutualFundsSummary();
        return View("Common", dsNew);
    }
    public async Task<IActionResult> ListOfStockPurchasesMutualFundsDetail()
    {
        DataSet dsNew = new DataSet();
        dsNew = await (new StockFileService()).GetMutualFundsDetail();
        return View("Common", dsNew);
    }
    // http://localhost:5278/stockprice/GetLatestPriceGivenStock?stockSymbol=MSFT
    public async Task<IActionResult> GetLatestPriceGivenStock(string stockSymbol = "MSFT")
    {
        DataSet dsNew = new DataSet();
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols

        Candle latestCandle = await (new YahooFinanceService()).GetLatestPriceGivenStock(stockSymbol);
        List<Candle> lstLatestCandle = new List<Candle>();
        lstLatestCandle.Add(latestCandle);
        DataTable dataTableLatestCandle = DataTableConverter.ToDataTable(lstLatestCandle);
        dsNew.Tables.Add(dataTableLatestCandle);
        return View("Common", dsNew);
    }
    // http://localhost:5278/stockprice/GetLatestNetworth?stockSymbol=MSFT
    public async Task<String> GetLatestNetworth(string stockSymbol = "MSFT")
    {
        var stockNetworthGivenDate = await (new StockNetworthService()).GetLatestNetworthForGivenDate(stockSymbol, DateTime.Now);
        string networthDetails = System.Text.Json.JsonSerializer.Serialize(stockNetworthGivenDate);
        _logger.LogInformation(networthDetails);
        return networthDetails;
    }
    // http://localhost:5278/stockprice/GetLatestNetworthForGivenDate?stockSymbol=MSFT&givenDateString=2023-10-01
    public async Task<StockNetworth> GetLatestNetworthForGivenDate(string stockSymbol = "MSFT", string givenDateString = null)
    {
        DateTime givenDate;
        if (string.IsNullOrEmpty(givenDateString) || !DateTime.TryParse(givenDateString, out givenDate))
        {
            givenDate = DateTime.Now;
        }
        var stockNetworthGivenDate = await (new StockNetworthService()).GetLatestNetworthForGivenDate(stockSymbol, givenDate);

        // Log the calculated net worth
        string networthValue = System.Text.Json.JsonSerializer.Serialize(stockNetworthGivenDate);

        return stockNetworthGivenDate;
    }
 // http://localhost:5278/stockprice/GetMutualFundNAV
public async Task<List<NAVData>> GetMutualFundNAV()
    {
        var navData = await (new MutualFundService()).GetMutualFundNAV();
        return navData;
    }
    public async Task<IActionResult> GetHistoricalPriceGivenStock()
    {
        DataSet dsNew = new DataSet();
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols
        var periods = new List<YahooFinanceApi.Period>
        {
            YahooFinanceApi.Period.Daily,
            YahooFinanceApi.Period.Weekly,
            YahooFinanceApi.Period.Monthly
        }; // List of periods

        var historicalData = new Dictionary<string, Dictionary<YahooFinanceApi.Period, List<Candle>>>();

        foreach (var symbol in symbols)
        {
            var periodData = new Dictionary<YahooFinanceApi.Period, List<Candle>>();
            foreach (var period in periods)
            {
                var candles = await (new YahooFinanceService()).GetHistoricalPriceGivenStock(
                    symbol,
                    new DateTime(2015, 1, 1),
                    new DateTime(2025, 7, 1),
                    period
                );

                periodData[period] = candles;
            }
            historicalData[symbol] = periodData;
        }
        return View("Common", dsNew);
    }
}