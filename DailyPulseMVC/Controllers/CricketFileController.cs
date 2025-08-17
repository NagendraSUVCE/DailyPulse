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
    public async Task<String> SeriesResults()
    {
        string ping = "CricketFileController Index Connected.";
        _logger.LogInformation(ping);
        return ping;
    }
}