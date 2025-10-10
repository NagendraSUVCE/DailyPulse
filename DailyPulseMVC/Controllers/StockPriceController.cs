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

    private readonly StockFileService _stockFileService;
    private readonly StockNetworthService _stockNetworthService;
    private readonly YahooFinanceService _yahooFinanceService;
    private readonly MutualFundService _mutualFundService;

    public StockPriceController(ILogger<Daily15MinLogController> logger)
    {
        _logger = logger;
        if (_stockFileService == null)
            _stockFileService = new StockFileService();
        if (_stockNetworthService == null)
            _stockNetworthService = new StockNetworthService();
        if (_yahooFinanceService == null)
            _yahooFinanceService = new YahooFinanceService();
        if (_mutualFundService == null)
            _mutualFundService = new MutualFundService();
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

        dsNew = await _stockFileService.GetMSFTPurchasesFromPayslipSummarized();
        return View("Common", dsNew);
    }
    //http://localhost:5278/stockprice/ListOfPurchases
    
    public async Task<IActionResult> ListOfPurchases()
    {
        DataSet dsNew = new DataSet();
        var lstPurchases = await _stockFileService.GetAllPurchases();
        DataTable dataTableLatestCandle = DataTableConverter.ToDataTable(lstPurchases);
        dsNew.Tables.Add(dataTableLatestCandle);
        return View("Common", dsNew);
    }
    public async Task<IActionResult> ListOfStockPurchasesMutualFundsDetail()
    {
        DataSet dsNew = new DataSet();
        dsNew = await _stockFileService.GetMutualFundsDetail();
        return View("Common", dsNew);
    }
    // http://localhost:5278/stockprice/GetLatestPriceGivenStock?stockSymbol=MSFT
    public async Task<IActionResult> GetLatestPriceGivenStock(string stockSymbol = "MSFT")
    {
        DataSet dsNew = new DataSet();
        var symbols = new List<string> { "MSFT", "0P0000XVJQ.BO" }; // List of stock symbols
        /*
            HDFC Mid Cap Dir Gr         (0P0000XW8F.BO)     118989
        */
        Candle latestCandle = await _yahooFinanceService.GetLatestPriceGivenStock(stockSymbol);
        List<Candle> lstLatestCandle = new List<Candle>();
        lstLatestCandle.Add(latestCandle);
        DataTable dataTableLatestCandle = DataTableConverter.ToDataTable(lstLatestCandle);
        dsNew.Tables.Add(dataTableLatestCandle);
        return View("Common", dsNew);
    }
    // http://localhost:5278/stockprice/GetLatestNetworth?stockSymbol=MSFT
    public async Task<Object> GetLatestNetworth(string stockSymbol = "MSFT")
    {
        var stockNetworthGivenDate = await _stockNetworthService.GetLatestNetworthForGivenDate(stockSymbol, DateTime.Now);
        string networthDetails = System.Text.Json.JsonSerializer.Serialize(stockNetworthGivenDate);
        _logger.LogInformation(networthDetails);
        return stockNetworthGivenDate;
    }
    // http://localhost:5278/stockprice/GetLatestNetworthForGivenDate?stockSymbol=MSFT&givenDateString=2023-10-01
    public async Task<StockNetworth> GetLatestNetworthForGivenDate(string stockSymbol = "MSFT", string givenDateString = null)
    {
        DateTime givenDate;
        if (string.IsNullOrEmpty(givenDateString) || !DateTime.TryParse(givenDateString, out givenDate))
        {
            givenDate = DateTime.Now;
        }
        var stockNetworthGivenDate = await _stockNetworthService.GetLatestNetworthForGivenDate(stockSymbol, givenDate);

        // Log the calculated net worth
        string networthValue = System.Text.Json.JsonSerializer.Serialize(stockNetworthGivenDate);

        return stockNetworthGivenDate;
    }
    // http://localhost:5278/stockprice/GetMutualFundNAV
    public async Task<List<NAVData>> GetMutualFundNAV()
    {
        var navData = await _mutualFundService.GetMutualFundNAV();
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
                var candles = await _yahooFinanceService.GetHistoricalPriceGivenStock(
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