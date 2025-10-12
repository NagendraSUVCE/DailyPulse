using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.DailyLog;
using YahooFinanceApi; // Add the namespace for Candle type

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
}