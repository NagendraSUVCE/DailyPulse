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
    public async Task<string> GetHtmlContentsFromUrl(string url)
    {
        using var client = new HttpClient();

        // Add headers to look like a browser
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        client.DefaultRequestHeaders.Add("Referer", "https://www.google.com");

        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        return html;
    }
}
