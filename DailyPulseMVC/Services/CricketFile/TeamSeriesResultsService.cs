using HtmlAgilityPack;

public class TeamSeriesResultsService
{
    private string baseUrl = "https://www.espncricinfo.com";

    public async Task<List<TeamSeriesResults>> GetTeamSeriesResults(string TeamSeriesMasterLink)
    {
        string teamSeriesMasterHtml = string.Empty;
        List<TeamSeriesResults> teamSeriesResults = new List<TeamSeriesResults>();
        try
        {
            teamSeriesMasterHtml = await GetWebsiteContentsAsync(TeamSeriesMasterLink);
        }
        catch (Exception ex)
        {
            string error = ex.Message;
            throw ex;
        }
        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(teamSeriesMasterHtml);
        HtmlNode docNode = htmlDocument?.DocumentNode;
        string xPath = @"//script[contains(@id,""__NEXT_DATA__"")]";
        var nodes = docNode.SelectNodes(xPath);
        HtmlNode tempNode = null;
        foreach (var item in nodes)
        {
            TeamSeriesResults obj = new TeamSeriesResults();
            obj.TeamSeriesMasterLink = TeamSeriesMasterLink;
            obj.SeriesTournamentName = teamSeriesMasterHtml;
            teamSeriesResults.Add(obj);
        }
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

    private async Task<String> GetWebsiteContentsAsync(string url)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.espncricinfo.com/records/trophy/team-series-results/indian-premier-league-117");
        request.Headers.Add("Cookie", "SWID=32aa4cca-8ed8-44e8-b251-6a3b3dd10826; bm_s=YAAQlMAzuEGvVpeZAQAACKmPzATO46AcPidoufUHRQ9QjJWrw76ywmx5/5QDnLcFFU0DCDcX4GuoqMNh+QWJeMWCialR1az3Hbtu+hJ+ILGhtxSuFSEzRoJ3Sa3kdI+mv6MYviG/Gb+DyF3XRSGmd083XR9u7ZuPnW/jycRQwVrRrvAYx2Swak55TKvj9D+E56tRNU0bvLzLqWVA3GXAr6Y7nJ5EELmdOTwAJNi4VOzrHR5v2b10ChegNWXUajsfSSD3b+87X3pQynLLxIvcX5/g4HnreZy9oAfI+WbYzAbcl0X3ekA8qHCSIH7tXZiuaF/tLCg/Rwfzsd7RxS/sLA13kQDR+aMNyZ4v2IuNS4QBRqb9oU1j/X+/M9MRURopwYLZaQazxJd9yeG502G7hCq8Kcd/qxeDLegje+DxQm9Ehz35piD4TNwiuXcc3H1o6kBLHbh3Fffb6hMWj/dBEr88/KrgHqAJUxoZnJjFHNj2xU79BGcBs5+agSmLzZQsjA6TRHQMUTQmVGIkc+Zynw3XyoQY/ZnhxFNzE40bROuDC8XWc22hlEnKgbg=; bm_so=A8658B3A08D0CB63F3AA4CEF4744B72FBDFF0C5A8B55525ABB73D0B1123A9858~YAAQbMAzuF5/MpSZAQAAJJbKyQUn+4AnD5H1gjW4W9YV/6/kjLcLSRe5Aa13DiSEFRGmiJDQ5lPTwn9vn7kuwVO/5x7P20DfgSWyHK526zfcZjVLQls7/oSZZwZsckQ4mhsWN57Ojh94SUMgxdhP5jD5424DeDNvRUdagh2SDNgvL2b4ItEboPVSyXyXClXKU1XeMfaE6uQfJNpt6smiMoH7SnxmDtEyuoZ1BY46skXuLm7WuEN0/0/8uLhFwnY4a2hupM8n10W8LV+lv/1cMw9lp0dAsW9Mt+JEyvERJXeiAFvfTqVpXpaVsrDmKGZ64Q+Z1g4JSxYYYckyD4iIl+X7UJzif8Z2rizojxqtQatRd/1VCa+9zrextc70Wi00r702DE+Kvr+H4yyTXN7TdkdzLXu+u7olrSUoBk3lfFRPatAk3cfjl46d3Z81cuIiM5D63rYL/tAAc4QOFkfN7QAFLmnx; bm_ss=ab8e18ef4e; country=in; edition=espncricinfo-en-in; edition-view=espncricinfo-en-in; region=unknown; _dcf=1; connectionspeed=full");
        request.Headers.Add("accept-language", "en-GB,en-US;q=0.9,en;q=0.8");
        request.Headers.Add("priority", "u=0, i");
        request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"macOS\"");
        request.Headers.Add("sec-fetch-dest", "document");
        request.Headers.Add("sec-fetch-mode", "navigate");
        request.Headers.Add("sec-fetch-site", "none");
        request.Headers.Add("sec-fetch-user", "?1");
        request.Headers.Add("upgrade-insecure-requests", "1");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string str = await response.Content.ReadAsStringAsync();
        return str;
    }
}