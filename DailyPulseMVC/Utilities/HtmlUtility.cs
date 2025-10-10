

public class HtmlUtility
{

    public Task<String> GetWebsiteContents(string url)
    {
        return Task.Run(() =>
            {
                using (HttpClient client = new HttpClient())
                {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0");
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
                }
            });  }
}