using HtmlAgilityPack;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public class TeamSeriesResultsService
{
    private string baseUrl = "https://www.espncricinfo.com";

    public async Task<List<TeamSeriesResults>> GetTeamSeriesResults(string TeamSeriesMasterLink)
    {
        string teamSeriesMasterHtml = string.Empty;
        List<TeamSeriesResults> teamSeriesResults = new List<TeamSeriesResults>();
        try
        {
            teamSeriesMasterHtml = await GetWebsiteContents(TeamSeriesMasterLink);
        }
        catch (Exception ex)
        {
            string error = ex.Message;
            throw ex;
        }
        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(teamSeriesMasterHtml);
        TeamSeriesResults obj = new TeamSeriesResults();
            obj.TeamSeriesMasterLink = TeamSeriesMasterLink;
            obj.SeriesTournamentName = teamSeriesMasterHtml;
            teamSeriesResults.Add(obj);
       /* HtmlNode docNode = htmlDocument?.DocumentNode;
        string xPath = @"//script[contains(@id,""__NEXT_DATA__"")]";
        var nodes = docNode.SelectNodes(xPath);
        HtmlNode tempNode = null;
        foreach (var item in nodes)
        {
            
        }  /*/
        return teamSeriesResults;
    }


    private Task<String> GetWebsiteContents(string url)
    {
        return Task.Run(() =>
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authority", "www.espncricinfo.com");
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        });
    }
}