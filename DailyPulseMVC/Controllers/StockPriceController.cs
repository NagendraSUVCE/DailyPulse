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
    //http://localhost:5278/stockprice/ListOfPurchasesMonthlySummary
        public async Task<IActionResult> ListOfPurchasesMonthlySummary()
    {
        DataSet dsNew = new DataSet();
        var lstPurchases = await _stockFileService.GetAllPurchases();
        // Group purchases by year and month, considering only TotalPurchaseValue > 0
        var groupedByYearMonth = lstPurchases
            .Where(p => p.TotalPurchaseValue > 0)
            .GroupBy(p => new { Year = p.PurchaseDate.GetValueOrDefault().Year, Month = p.PurchaseDate.GetValueOrDefault().Month })
            .Select(g => new
            {
            Year = g.Key.Year,
            Month = g.Key.Month,
            TotalPurchaseValue = g.Sum(p => p.TotalPurchaseValue)
            })
            .ToList();

        // Create the first DataTable (years as columns)
        DataTable yearlySummaryTable = new DataTable("YearlySummary");
        yearlySummaryTable.Columns.Add("Description", typeof(string));
        var distinctYears = groupedByYearMonth.Select(g => g.Year).Distinct().OrderBy(y => y);
        foreach (var year in distinctYears)
        {
            yearlySummaryTable.Columns.Add(year.ToString(), typeof(decimal));
        }
        yearlySummaryTable.Columns.Add("TotalInvestment", typeof(decimal)); // Add TotalInvestment column

        DataRow yearlyRow = yearlySummaryTable.NewRow();
        yearlyRow["Description"] = "Total Purchase Value";
        decimal totalInvestment = 0; // Variable to calculate total investment
        foreach (var year in distinctYears)
        {
            var yearlyTotal = groupedByYearMonth
            .Where(g => g.Year == year && g.TotalPurchaseValue > 0)
            .Sum(g => g.TotalPurchaseValue);
            yearlyRow[year.ToString()] = yearlyTotal;
            totalInvestment += yearlyTotal.GetValueOrDefault(); // Accumulate total investment
        }
        yearlyRow["TotalInvestment"] = totalInvestment; // Set total investment value
        yearlySummaryTable.Rows.Add(yearlyRow);

        // Create the second DataTable (months as columns, years as rows)
        DataTable monthlySummaryTable = new DataTable("MonthlySummary");
        monthlySummaryTable.Columns.Add("Year", typeof(int));
        for (int i = 1; i <= 12; i++)
        {
            monthlySummaryTable.Columns.Add(new DateTime(1, i, 1).ToString("MMM"), typeof(decimal));
        }
        monthlySummaryTable.Columns.Add("Total", typeof(decimal));

        foreach (var year in distinctYears)
        {
            DataRow monthlyRow = monthlySummaryTable.NewRow();
            monthlyRow["Year"] = year;
            decimal yearlyTotal = 0;
            for (int month = 1; month <= 12; month++)
            {
            var monthlyTotal = groupedByYearMonth
                .Where(g => g.Year == year && g.Month == month)
                .Sum(g => g.TotalPurchaseValue);
            monthlyRow[new DateTime(1, month, 1).ToString("MMM")] = monthlyTotal;
            yearlyTotal += monthlyTotal.Value;
            }
            monthlyRow["Total"] = yearlyTotal;
            monthlySummaryTable.Rows.Add(monthlyRow);
        }

        // Add the tables to the dataset
        dsNew.Tables.Add(yearlySummaryTable);
        dsNew.Tables.Add(monthlySummaryTable);

        DataTable dataTableLatestCandle = DataTableConverter.ToDataTable(lstPurchases);
        // dsNew.Tables.Add(dataTableLatestCandle);
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