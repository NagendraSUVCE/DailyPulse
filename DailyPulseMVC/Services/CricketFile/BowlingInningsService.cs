
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

public class BowlingInningsService
{


    public async Task<List<BowlingInnings>> Main(string url)
    {
        List<BowlingInnings> bowlingInningsList = new List<BowlingInnings>();
        //string url = "https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=batted_score;page=1;template=results;trophy=117;type=bowling;view=innings";
        //url = @"https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;template=results;trophy=117;type=bowling;view=innings;size=200;page=2;";

        using HttpClient client = new HttpClient();
        var html = await client.GetStringAsync(url);

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Select the table with the specific caption
        var table = doc.DocumentNode.SelectSingleNode("//table[caption[text()='Innings by innings list']]");
        BowlingInnings innings = null;
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
                if (cells != null && cells.Count >= 5)
                {
                    innings = new BowlingInnings();

                    try
                    {
                        innings.PlayerName = cells[0].InnerText.Trim();
                        if (cells[1].InnerText.Trim().Contains("DNB"))
                        {
                            innings.Desc = "DNB";
                        }
                        else
                        {
                            innings.Overs = cells[1].InnerText.Trim() != "-" ? decimal.Parse(cells[1].InnerText.Trim()) : 0;
                        }
                        innings.Maidens = cells[2].InnerText.Trim() != "-" ? int.Parse(cells[2].InnerText.Trim()) : 0;
                        innings.Runs = cells[3].InnerText.Trim() != "-" ? int.Parse(cells[3].InnerText.Trim()) : 0;
                        innings.Wickets = cells[4].InnerText.Trim() != "-" ? int.Parse(cells[4].InnerText.Trim()) : 0;
                        innings.Econ = cells[5].InnerText.Trim() != "-" ? decimal.Parse(cells[5].InnerText.Trim()) : 0;
                        innings.Inns = int.Parse(cells[6].InnerText.Trim());

                        innings.OppTeam = cells[8].InnerText.Trim();

                        innings.Ground = cells[9].InnerText.Trim();

                        innings.DateString = cells[10].InnerText.Trim();
                        if (DateTime.TryParseExact(cells[10].InnerText.Trim(), "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                        {
                            innings.StartDate = parsedDate;
                        }
                        else if (DateTime.TryParseExact(cells[10].InnerText.Trim(), "d MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate2))
                        {
                            innings.StartDate = parsedDate2;
                        }
                        innings.DateString = "date" + innings.DateString;
                        //innings.OppTeamIndexCI = System.Net.WebUtility.HtmlEncode(cells[9].InnerHtml.Trim());
                        try
                        {
                            innings.PlayerIndexCI = cells[0].ChildNodes[0].Attributes["href"].Value;
                            innings.GroundIndexCI = cells[9].ChildNodes[0].Attributes["href"].Value;
                            innings.OppTeamIndexCI = cells[8].ChildNodes[1].Attributes["href"].Value;
                            // innings.Temp = System.Net.WebUtility.HtmlEncode(cells[12].OuterHtml.Trim());
                            innings.TempAgain = cells[11].ChildNodes[0].Attributes["onmouseover"].Value;
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
                    bowlingInningsList.Add(innings);
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
        return bowlingInningsList;
    }
}
