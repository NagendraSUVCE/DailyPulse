
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

public class BattingInningsService
{
    public async Task<List<BattingInnings>> Main(string url)
    {
        List<BattingInnings> battingInningsList = new List<BattingInnings>();
        //string url = "https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=batted_score;page=1;template=results;trophy=117;type=batting;view=innings";
        //url = @"https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;template=results;trophy=117;type=batting;view=innings;size=200;page=2;";

        using HttpClient client = new HttpClient();
        var html = await client.GetStringAsync(url);

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
                        // innings.ForTeam = cells[1].InnerText.Trim();
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
                        if (DateTime.TryParseExact(cells[11].InnerText.Trim(), "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                        {
                            innings.StartDate = parsedDate;
                        }
                        else if (DateTime.TryParseExact(cells[11].InnerText.Trim(), "d MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate2))
                        {
                            innings.StartDate = parsedDate2;
                        }
                        innings.DateString = "date" + innings.DateString;
                        innings.OppTeam = cells[9].InnerText.Trim();
                        //innings.OppTeamIndexCI = System.Net.WebUtility.HtmlEncode(cells[9].InnerHtml.Trim());
                        try
                        {
                            innings.PlayerIndexCI = cells[0].ChildNodes[0].Attributes["href"].Value;
                            innings.GroundIndexCI = cells[10].ChildNodes[0].Attributes["href"].Value;
                            innings.OppTeamIndexCI = cells[9].ChildNodes[1].Attributes["href"].Value;
                            // innings.Temp = System.Net.WebUtility.HtmlEncode(cells[12].OuterHtml.Trim());
                            innings.TempAgain = cells[12].ChildNodes[0].Attributes["onmouseover"].Value;
                            innings.TempAgain = innings.TempAgain.Replace("menuLayers.show(", "").Replace(", event); window.status='investigate this query'; return true", "");
                            innings.TempAgain = innings.TempAgain.Replace("'", "").Trim();
                            // //*[@id="engine-dd2"]/ul/li[6]/a
                            innings.MatchIndexCI = doc.DocumentNode.SelectSingleNode("//div[@id='" + innings.TempAgain + "']/ul/li[6]/a").Attributes["href"].Value;
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
        return battingInningsList;
    }
}
