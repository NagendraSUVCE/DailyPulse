using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.DailyLog;
using YahooFinanceApi;
using Org.BouncyCastle.Crypto.Signers; // Add the namespace for Candle type

namespace DailyPulseMVC.Controllers;

public class CricketFileController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<CricketFileController> _logger;
    private TeamSeriesResultsService _teamSeriesResultsService = new TeamSeriesResultsService();

    public CricketFileController(ILogger<CricketFileController> logger)
    {
        _logger = logger;
    }
    public async Task<String> Index()
    {
        string ping = "CricketFileController Index Connected.";
        _logger.LogInformation(ping);
        return ping;
    }
    public async Task<List<TeamSeriesResults>> TeamSeriesResults(string url)
    {
        // string iplSeries = "https://www.espncricinfo.com/records/trophy/team-series-results/indian-premier-league-117";
        // iplSeries = @"https://www.espncricinfo.com/ci/engine/series/60260.html";
        return await _teamSeriesResultsService.GetTeamSeriesResults(url);
    }
    // http://localhost:5278/CricketFile/GetHtmlContentsFromUrl?url=https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;template=results;trophy=117;type=batting;view=innings;size=200;page=2;
    public async Task<IActionResult> GetHtmlContentsFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            url = "https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;template=results;trophy=117;type=batting;view=innings;size=200;page=2;";
        }
        BattingInningsService battingInningsService = new BattingInningsService();
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine($"Bowling Iteration number: {i + 1}");
            // await battingInningsService.LoopThroughDates("Bowling");
            Console.WriteLine($"Batting Iteration number: {i + 1}");
            // await battingInningsService.LoopThroughDates("Batting");
        }
        DataSet dataSet = new DataSet();
        if (url.Contains("type=batting"))
        {
            List<BattingInnings> battingInnings = new List<BattingInnings>();
            battingInnings = await battingInningsService.Main(url);
            DataTable dtBattingDetails = DataTableConverter.ToDataTable(battingInnings);
            dataSet.Tables.Add(dtBattingDetails);
        }
        if (url.Contains("type=bowling"))
        {
            List<BowlingInnings> bowlingInnings = new List<BowlingInnings>();
            BowlingInningsService bowlingInningsService = new BowlingInningsService();
            bowlingInnings = await bowlingInningsService.Main(url);
            DataTable dtBowlingDetails = DataTableConverter.ToDataTable(bowlingInnings);
            dataSet.Tables.Add(dtBowlingDetails);
        }
        if (url.Contains("type=team"))
        {
            List<TeamResultScore> teamResultScores = new List<TeamResultScore>();
            TeamScoreService teamScoreService = new TeamScoreService();
            teamResultScores = await teamScoreService.GetTeamResultScoreList(url);
            DataTable dtTeamScores = DataTableConverter.ToDataTable(teamResultScores);
            dataSet.Tables.Add(dtTeamScores);
        }
        return View("Common", dataSet);
    }
    public async Task<IActionResult> GetAllPaginations()
    {
        BattingInningsService cricinfoPaginationService = new BattingInningsService();
        DataSet dataSet = new DataSet();
        List<CricinfoPagination> paginations = new List<CricinfoPagination>();
        paginations = await cricinfoPaginationService.GetAllPages();
        DataTable dtPaginations = DataTableConverter.ToDataTable(paginations);
        dataSet.Tables.Add(dtPaginations);
        return View("Common", dataSet);
    }
    public async Task<IActionResult> SaveEveryStatsAsHtml()
    {
        BattingInningsService cricinfoPaginationService = new BattingInningsService();
        DataSet dataSet = new DataSet();
        List<CricinfoPagination> paginations = new List<CricinfoPagination>();
        paginations = await cricinfoPaginationService.GetAllPages();
        foreach (var pagination in paginations)
        {
            InningsService inningsService = new InningsService();
            inningsService.cricinfoPaginations = new List<CricinfoPagination> { pagination };
            await inningsService.GetAllHtmls();
        }
        DataTable dtPaginations = DataTableConverter.ToDataTable(paginations);
        dataSet.Tables.Add(dtPaginations);
        return View("Common", dataSet);
    }
}
