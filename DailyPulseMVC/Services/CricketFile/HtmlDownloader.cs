
using Microsoft.Playwright;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class PlaywrightProgram
{
    public async Task Main()
    {
        // Launch Playwright
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false if you want to see the browser
        });

        var page = await browser.NewPageAsync();

        // Navigate to ESPNcricinfo IPL results page
        var url = "https://www.espncricinfo.com/records/trophy/team-series-results/indian-premier-league-117";
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });


                // Get full HTML
        var html = await page.ContentAsync();

        // Wait for the table to appear
        await page.WaitForSelectorAsync("table.ds-w-full");

        // Extract rows
        var rows = await page.QuerySelectorAllAsync("table.ds-w-full tbody tr");

        var sb = new StringBuilder();
        sb.AppendLine("Season,Winner,Margin");

        foreach (var row in rows)
        {
            var cells = await row.QuerySelectorAllAsync("td");
            if (cells.Count >= 4)
            {
                var season = (await cells[1].InnerTextAsync()).Trim();
                var winner = (await cells[2].InnerTextAsync()).Trim();
                var margin = (await cells[3].InnerTextAsync()).Trim();

                sb.AppendLine($"{EscapeCsv(season)},{EscapeCsv(winner)},{EscapeCsv(margin)}");
            }
        }

        // Save CSV
        var filePath = Path.Combine(Environment.CurrentDirectory, "ipl_series_results.csv");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

        Console.WriteLine($"✅ CSV saved to: {filePath}");
    }

    private string EscapeCsv(string value)
    {
        if (value.Contains(",") || value.Contains("\""))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        return value;
    }
}
