using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DailyPulseMVC.Services;

[Route("fitbit")]
public class FitbitAuthController : Controller
{
    private string FitbitClientId = "";
    private string FitbitClientSecret = "";
    private string FitbitTokenFilePath = "";
    private string FitbitRedirectUri = "";
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        FitbitApiClient fitbitApiClient = new FitbitApiClient();
        FitbitClientId = fitbitApiClient.FitbitClientId;
        FitbitClientSecret = fitbitApiClient.FitbitClientSecret;
        FitbitRedirectUri = fitbitApiClient.FitbitRedirectUri;
        FitbitTokenFilePath = fitbitApiClient.FitbitTokenFileName;
           string scope = "activity heartrate location nutrition profile settings sleep social weight";
        string url = $"https://api.fitbit.com/oauth2/authorize?response_type=code&client_id={FitbitClientId}&redirect_uri={HttpUtility.UrlEncode(FitbitRedirectUri)}&scope={HttpUtility.UrlEncode(scope)}";
     
        string authUrl = url;
        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code)
    {
         FitbitApiClient fitbitApiClient = new FitbitApiClient();
        FitbitClientId = fitbitApiClient.FitbitClientId;
        FitbitClientSecret = fitbitApiClient.FitbitClientSecret;
        FitbitRedirectUri = fitbitApiClient.FitbitRedirectUri;
        FitbitTokenFilePath = fitbitApiClient.FitbitTokenFileName;
        if (string.IsNullOrEmpty(code))
            return BadRequest("Authorization code missing.");

        using var client = new HttpClient();
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{FitbitClientId}:{FitbitClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var content = new StringContent(
            $"client_id={FitbitClientId}&grant_type=authorization_code&redirect_uri={HttpUtility.UrlEncode(FitbitRedirectUri)}&code={code}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );

        var response = await client.PostAsync("https://api.fitbit.com/oauth2/token", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // await System.IO.File.WriteAllTextAsync(FitbitTokenFilePath, responseBody);
         await fitbitApiClient.UploadTokenDataToGraphAsync(responseBody);
        // You can deserialize and store the tokens here
        return Content("Token Response:\n" + responseBody, "text/plain");
    }
    [HttpGet("getstepsdata")]
    public async Task GetStepsData()
    {
        var fitbitService = new FitbitService();
        List<StepsData> lstStepsData = await fitbitService.GetAndSaveLatestStepsData();
        var last7DaysData = lstStepsData
            .Where(data => data.DateOfActivity >= DateTime.Now.AddDays(-7))
            .OrderByDescending(data => data.DateOfActivity)
            .ToList();

        var jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(last7DaysData);

        Response.ContentType = "application/json";
        await Response.WriteAsync(jsonResult);
        
    }
    [HttpGet("getstepseveryminute")]
    public async Task GetStepsEveryMinute()
    {
        var fitbitService = new FitbitService();
        bool stepsEveryminute = await fitbitService.TryFetchStepsEveryMinuteEvery15Minute();
        Response.ContentType = "application/json";
        await Response.WriteAsync("success");
        
    }
}
