
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DailyPulseMVC.Services.CricketFile;
using System.Threading.Tasks.Dataflow;


public class BattingInningsService
{
    private readonly DownloadWebService _downloadWebService;
    string folderPath = "";
    public BattingInningsService()
    {
        _downloadWebService = new DownloadWebService();
        folderPath = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/CricketFile";
    }
    public async Task LoopThroughDates()
    {
        DateTime startDate = new DateTime(2008, 4, 1);
        DateTime endDate = DateTime.Now;
        int iterationCount = 0;
            string logFileName = "DownloadLog.csv";
            string logFilePath = Path.Combine(folderPath, logFileName);
        while (startDate <= endDate && iterationCount < 10)
        {
            string fileName = $"BattingInnings_{startDate:yyyyMMdd}.csv";
            if (File.Exists(Path.Combine(folderPath, fileName)))
            {
                startDate = startDate.AddDays(1);
                continue;
            }
            string url = $"https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;size=200;spanval1=span;template=results;trophy=117;view=innings;spanmax1={startDate:dd+MMM+yyyy};spanmin1={startDate:dd+MMM+yyyy};type=batting;";
            // Check if the URL is already present in the log file
            if (File.Exists(logFilePath))
            {
                var existingUrls = File.ReadAllLines(logFilePath)
                                       .Skip(1) // Skip the header row
                                       .Select(line => line.Split(',')[1]) // Extract the URL column
                                       .ToHashSet();

                if (existingUrls.Contains(url))
                {
                    Console.WriteLine($"URL already processed: {url}");
                    startDate = startDate.AddDays(1);
                    continue;
                }
            }
            List<BattingInnings> battingInnings = await Main(url);
            await Task.Delay(5000);
            Console.WriteLine(url);
            startDate = startDate.AddDays(1);
            if (battingInnings.Count > 0)
            {

                SaveBattingInningsToCsv(battingInnings, fileName);
                iterationCount++;
            }

            try
            {
                bool logFileExists = File.Exists(logFilePath);

                using (var logWriter = new StreamWriter(logFilePath, append: true))
                {
                    if (!logFileExists)
                    {
                        logWriter.WriteLine("SlNo,URL,FileExists,Status,DateTime");
                    }

                    string fileExists = battingInnings.Count > 0 ? "True" : "False";
                    string status = "Completed";

                    logWriter.WriteLine($"{iterationCount + 1},{url},{fileExists},{status},{DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
        /*
        https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;size=200;spanval1=span;template=results;trophy=117;view=innings;spanmax1=18+Apr+2008;spanmin1=18+Apr+2008;type=batting;
        */
    }
    public void SaveBattingInningsToCsv(List<BattingInnings> battingInningsList, string fileName)
    {
        string filePath = Path.Combine(folderPath, fileName);

        try
        {
            Directory.CreateDirectory(folderPath);

            if (!File.Exists(filePath))
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("PlayerName,Runs,RunsNotOut,Mins,BallsFaced,Four4s,Sixers,StrikeRate,Inns,ForTeam,OppTeam,Ground,DateString,StartDate,PlayerIndexCI,GroundIndexCI,OppTeamIndexCI,TempAgain,MatchIndexCI");

                    foreach (var innings in battingInningsList)
                    {
                        writer.WriteLine($"{innings.PlayerName},{innings.Runs},{innings.RunsNotOut},{innings.Mins},{innings.BallsFaced},{innings.Four4s},{innings.Sixers},{innings.StrikeRate},{innings.Inns},{innings.ForTeam},{innings.OppTeam},{innings.Ground},{innings.DateString},{innings.StartDate},{innings.PlayerIndexCI},{innings.GroundIndexCI},{innings.OppTeamIndexCI},{innings.TempAgain},{innings.MatchIndexCI}");
                    }
                }

                Console.WriteLine($"File saved successfully at {filePath}");
            }
            else
            {
                Console.WriteLine($"File already exists at {filePath}. No changes were made.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
        }
    }
    public async Task<List<BattingInnings>> Main(string url)
    {
        string dateString = String.Empty, tempAgain = string.Empty;
        List<BattingInnings> battingInningsList = new List<BattingInnings>();

        var html = await _downloadWebService.DownloadContentAsync(url);

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Select the table with the specific caption
        var table = doc.DocumentNode.SelectSingleNode("//table[caption[text()='Innings by innings list']]");
        BattingInnings innings = null;
        if (table != null)
        {
            // Extract rows
            var rows = table.SelectNodes(".//tr");
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//td");
                if (cells != null && cells.Count <= 1)
                {
                    if (innings != null)
                    {
                        innings.ForTeam = cells[0].InnerText.Trim();
                    }
                }
                if (cells != null && cells.Count >= 7)
                {
                    innings = new BattingInnings();

                    try
                    {
                        innings.PlayerName = cells[0].InnerText.Trim();
                        if (cells[1].InnerText.Trim().Contains("DNB"))
                        {
                            innings.RunsNotOut = "DNB";
                        }
                        else if (cells[1].InnerText.Trim().Contains("*"))
                        {
                            innings.RunsNotOut = "NO";
                            innings.Runs = int.Parse(cells[1].InnerText.Trim().Replace("*", ""));
                        }
                        else
                        {
                            innings.RunsNotOut = "OUT";
                            innings.Runs = int.Parse(cells[1].InnerText.Trim());
                        }
                        innings.Mins = cells[2].InnerText.Trim() != "-" ? int.Parse(cells[2].InnerText.Trim()) : 0;
                        innings.BallsFaced = cells[3].InnerText.Trim() != "-" ? int.Parse(cells[3].InnerText.Trim()) : 0;
                        innings.Four4s = cells[4].InnerText.Trim() != "-" ? int.Parse(cells[4].InnerText.Trim()) : 0;
                        innings.Sixers = cells[5].InnerText.Trim() != "-" ? int.Parse(cells[5].InnerText.Trim()) : 0;
                        innings.StrikeRate = cells[6].InnerText.Trim() != "-" ? decimal.Parse(cells[6].InnerText.Trim()) : 0;
                        innings.Inns = int.Parse(cells[7].InnerText.Trim());

                        innings.Ground = cells[10].InnerText.Trim();

                        innings.DateString = cells[11].InnerText.Trim();
                        innings.StartDate = ParseDateFromString(innings.DateString);
                        innings.DateString = "date" + innings.DateString;
                        // dateString = cells[11].InnerText.Trim();
                        /* if (DateTime.TryParseExact(dateString, "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                        {
                            innings.StartDate = parsedDate;
                        }
                        else if (DateTime.TryParseExact(dateString, "d MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate2))
                        {
                            innings.StartDate = parsedDate2;
                        } */
                        innings.OppTeam = cells[9].InnerText.Trim();
                        try
                        {
                            innings.PlayerIndexCI = cells[0].ChildNodes[0].Attributes["href"].Value;
                            innings.GroundIndexCI = cells[10].ChildNodes[0].Attributes["href"].Value;
                            innings.OppTeamIndexCI = cells[9].ChildNodes[1].Attributes["href"].Value;
                            tempAgain = cells[12].ChildNodes[0].Attributes["onmouseover"].Value;
                            tempAgain = GetMatchIndexCI(tempAgain);
                            innings.TempAgain = tempAgain;
                            innings.MatchIndexCI = doc.DocumentNode.SelectSingleNode("//div[@id='" + GetMatchIndexCI(tempAgain) + "']/ul/li[6]/a").Attributes["href"].Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        innings.PlayerName = "Error";
                        innings.PlayerIndexCI = ex.Message;
                        Console.WriteLine("Error parsing row: " + ex.Message);
                    }
                    battingInningsList.Add(innings);
                }
                if (cells != null)
                {
                    foreach (var cell in cells)
                    {
                        Console.Write(cell.InnerText.Trim() + "\t");
                    }
                    Console.WriteLine();
                }
            }
        }
        else
        {
            Console.WriteLine("Table not found.");
        }

        // Select the table with the specific caption
        var rowsWithData2 = doc.DocumentNode.SelectNodes(".//tr[contains(@class, 'data2')]");


        if (rowsWithData2 != null)
        {
            foreach (var row in rowsWithData2)
            {
                if (row.InnerHtml.Contains("Page "))
                {

                    var pageInfoText = row.InnerText.Trim();
                    var pageInfoParts = pageInfoText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pageInfoParts.Length >= 4 && int.TryParse(pageInfoParts[1], out int pageNumber) && int.TryParse(pageInfoParts[3], out int totalPages))
                    {
                        var pagination = new
                        {
                            PageNumber = pageNumber,
                            TotalPages = totalPages
                        };

                        Console.WriteLine($"Page Number: {pagination.PageNumber}, Total Pages: {pagination.TotalPages}");
                    }

                }
            }
        }
        return battingInningsList;
    }

    private DateTime ParseDateFromString(string dateString)
    {
        DateTime parsedDate;
        if (DateTime.TryParseExact(dateString, "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
        {
            return parsedDate;
        }
        else if (DateTime.TryParseExact(dateString, "d MMM yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
        {
            return parsedDate;
        }
        return DateTime.MinValue;
    }

    private string GetMatchIndexCI(string tempAgain)
    {
        tempAgain = tempAgain.Replace("menuLayers.show('", "");
        tempAgain = tempAgain.Replace(", event); window.status='investigate this query'; return true", "");
        tempAgain = tempAgain.Replace("'", "").Trim();
        return tempAgain;
    }
}
