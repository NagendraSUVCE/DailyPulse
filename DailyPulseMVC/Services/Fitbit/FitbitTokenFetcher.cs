using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

internal class FitbitTokenFetcher
{
    private static readonly string clientId = "YOUR_CLIENT_ID";
    private static readonly string clientSecret = "YOUR_CLIENT_SECRET";
    private static readonly string redirectUri = "YOUR_REDIRECT_URI";
    private static readonly string authorizationCode = "CODE_FROM_REDIRECT";

    private static async Task Main()
    {
        using var client = new HttpClient();
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var content = new StringContent(
            $"client_id={clientId}&grant_type=authorization_code&redirect_uri={redirectUri}&code={authorizationCode}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );

        var response = await client.PostAsync("https://api.fitbit.com/oauth2/token", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Token Response: " + responseBody);
    }
}
