
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

public class TeamScoreService
{
    public async Task<List<TeamResultScore>> GetTeamResultScoreList(string url)
    {
        List<TeamResultScore> teamResultScoresList = new List<TeamResultScore>();
        //string url = "https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=batted_score;page=1;template=results;trophy=117;type=batting;view=innings";
        //url = @"https://stats.espncricinfo.com/ci/engine/stats/index.html?class=6;filter=advanced;orderby=start;template=results;trophy=117;type=batting;view=innings;size=200;page=2;";

        using HttpClient client = new HttpClient();
        var html = await client.GetStringAsync(url);

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Select the table with the specific caption
        var table = doc.DocumentNode.SelectSingleNode("//table[caption[text()='Innings by innings list']]");
        TeamResultScore teamResultScoreObj = null;
        if (table != null)
        {
            // Extract rows
            var rows = table.SelectNodes(".//tr");
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//td");
                if (cells != null && cells.Count >= 8)
                {
                    teamResultScoreObj = new TeamResultScore();

                    try
                    {
                        teamResultScoreObj.Team = cells[0].InnerText.Trim();
                        teamResultScoreObj.Score = "score "+cells[1].InnerText.Trim();
                        teamResultScoreObj.Overs = cells[2].InnerText.Trim();
                        teamResultScoreObj.RPO = cells[3].InnerText.Trim();
                        teamResultScoreObj.Inns = cells[4].InnerText.Trim() != "-" ? int.Parse(cells[4].InnerText.Trim()) : 0;

                        teamResultScoreObj.Result = cells[6].InnerText.Trim() != "-" ? cells[6].InnerText.Trim() : "N/A";
                        teamResultScoreObj.OppTeam = cells[7].InnerText.Trim() != "-" ? cells[7].InnerText.Trim(): "N/A";
                        teamResultScoreObj.Ground = cells[8].InnerText.Trim() != "-" ? cells[8].InnerText.Trim(): "N/A";

                        teamResultScoreObj.DateString = cells[9].InnerText.Trim();
                        if (DateTime.TryParseExact(cells[9].InnerText.Trim(), "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None
                        , out DateTime parsedDate))
                        {
                            teamResultScoreObj.StartDate = parsedDate;
                        }
                        else if (DateTime.TryParseExact(cells[9].InnerText.Trim(), "d MMM yyyy", null, System.Globalization.DateTimeStyles.None
                        , out DateTime parsedDate2))
                        {
                            teamResultScoreObj.StartDate = parsedDate2;
                        }
                        teamResultScoreObj.DateString = "date " + teamResultScoreObj.DateString;
                        
                        //innings.OppTeamIndexCI = System.Net.WebUtility.HtmlEncode(cells[9].InnerHtml.Trim());
                        try
                        {
                            teamResultScoreObj.TeamIndexCI = cells[0].ChildNodes[0].Attributes["href"].Value;
                            teamResultScoreObj.OppTeamIndexCI = cells[7].ChildNodes[1].Attributes["href"].Value;

                            teamResultScoreObj.GroundIndexCI = cells[8].ChildNodes[0].Attributes["href"].Value;
                            
                            teamResultScoreObj.TempAgain = cells[10].ChildNodes[0].Attributes["onmouseover"].Value;
                            teamResultScoreObj.TempAgain = teamResultScoreObj.TempAgain.Replace("menuLayers.show(", "").Replace(", event); window.status='investigate this query'; return true", "");
                            teamResultScoreObj.TempAgain = teamResultScoreObj.TempAgain.Replace("'", "").Trim();
                            // //*[@id="engine-dd2"]/ul/li[6]/a
                            teamResultScoreObj.MatchIndexCI = doc.DocumentNode.SelectSingleNode("//div[@id='" + teamResultScoreObj.TempAgain + "']/ul/li[6]/a").Attributes["href"].Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        teamResultScoreObj.Team = "Error";
                        teamResultScoreObj.TeamIndexCI = ex.Message;
                        Console.WriteLine("Error parsing row: " + ex.Message);
                    }
                    teamResultScoresList.Add(teamResultScoreObj);
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
        return teamResultScoresList;
    }
}
