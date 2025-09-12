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

        List<StockPurchase> lstPurchases = await (new StockFileService()).GetStockPurchases();
        DataTable dataTablePurchases = DataTableConverter.ToDataTable(lstPurchases);
        dsNew.Tables.Add(dataTablePurchases);
        return View("Common", dsNew);
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