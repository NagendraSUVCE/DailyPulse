
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class StepsData
{
    [JsonProperty("dateTime")]
    public DateTime DateOfActivity { get; set; }

    [JsonProperty("value")]
    public int StepsValue { get; set; }
}
public class FitbitApiClient
{
    public string FitbitClientId { get; set; }
    public string FitbitClientSecret { get; set; }
    public string FitbitRedirectUri { get; set; }
    public string FitbitTokenFileName { get; set; }
    private string _txtPath;
    private string _tempFilePath;
    private string folderPath;
    public FitbitApiClient()
    {

        FitbitClientId = KeyVaultUtility.KeyVaultUtilityGetSecret("FitbitClientId");
        FitbitClientSecret = KeyVaultUtility.KeyVaultUtilityGetSecret("FitbitClientSecret");
        FitbitRedirectUri = "http://localhost:5278/fitbit/callback"; // Must match Fitbit app settings

        FitbitTokenFileName = "fitbittoken.txt"; // Path to store the access token
        _txtPath = $@"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/Fitbit/{FitbitTokenFileName}";
        _tempFilePath = $"temp_{FitbitTokenFileName}";
        folderPath = @"Nagendra/SelfCode/DatabaseInCSV/Fitbit";
    }

    public async Task<List<StepsData>> FetchWeeklyStepsAsync(DateTime startDate, DateTime endDate)
    {
        List<StepsData> stepsDataList = new List<StepsData>();
        //string tokenJson = await File.ReadAllTextAsync(FitbitTokenFileName);
        string tokenJson = await GetFitbitTokenFromJsonUsingGraph();
        var tokenObj = JObject.Parse(tokenJson);
        string accessToken = tokenObj["access_token"]?.ToString();
        string refreshToken = tokenObj["refresh_token"]?.ToString();
        stepsDataList = await TryFetchSteps(accessToken, startDate, endDate);

        bool success = stepsDataList != null && stepsDataList.Count > 0;
        if (!success)
        {
            Console.WriteLine("Access token failed. Attempting refresh...");
            string newToken = await RefreshAccessToken(refreshToken);
            if (newToken != null)
            {
                await UploadTokenDataToGraphAsync(newToken);
                var newAccessToken = JObject.Parse(newToken)["access_token"]?.ToString();
                stepsDataList = await TryFetchSteps(newAccessToken, startDate, endDate);
            }
            else
            {
                Console.WriteLine("Refresh failed. Please login interactively at /fitbit/login");
            }
        }
        DateTime currentDate = new DateTime(2016, 5, 27);
        while (currentDate <= endDate)
        {
            string fetchstepsEveryminute = await TryFetchStepsEveryMinute(accessToken, currentDate);
            await Task.Delay(1000); // 1-second delay
            currentDate = currentDate.AddDays(1);
        }
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
        return stepsDataList;
    }

    private async Task<List<StepsData>> TryFetchSteps(string accessToken, DateTime startDate, DateTime endDate)
    {
        List<StepsData> stepsDataList = new List<StepsData>();
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string url = $"https://api.fitbit.com/1/user/-/activities/steps/date/{startDate.ToString("yyyy-MM-dd")}/{endDate.ToString("yyyy-MM-dd")}.json";

            // url = $"https://api.fitbit.com/1/user/{0}{1}/date/{2}/{3}.json", encodedUserId, timeSeriesResourceType.GetStringValue(), baseDate.ToFitbitFormat(), endDateOrPeriod);


            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return stepsDataList;

            var data = await response.Content.ReadAsStringAsync();
            stepsDataList = JObject.Parse(data)["activities-steps"]
                .ToObject<List<StepsData>>();

            Console.WriteLine("Steps Data:\n" + data);
            return stepsDataList;
        }
        catch
        {
            return stepsDataList;
        }
    }

    private async Task<string> TryFetchStepsEveryMinute(string accessToken, DateTime activityDate)
    {
        string dataPath = $@"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/Fitbit/EveryMinuteStepsData/";
        string fileName = $"StepsEveryMinute_{activityDate.ToString("yyyy-MM-dd")}.json";
        string filePath = Path.Combine(dataPath, fileName);

        if (File.Exists(filePath))
        {
            Console.WriteLine($"File already exists: {filePath}");
            return await File.ReadAllTextAsync(filePath);
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string url = $"https://api.fitbit.com/1/user/-/activities/steps/date/{activityDate.ToString("yyyy-MM-dd")}/1d/1min.json";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch data from Fitbit API. Status Code: {response.StatusCode}");
                return "";
            }

            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Steps Data:\n" + data);

            Directory.CreateDirectory(dataPath); // Ensure the directory exists
            await File.WriteAllTextAsync(filePath, data);

            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching steps data: {ex.Message}");
            return "";
        }
    }

    private async Task<string> RefreshAccessToken(string refreshToken)
    {
        using var client = new HttpClient();
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{FitbitClientId}:{FitbitClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync("https://api.fitbit.com/oauth2/token", content);

        return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
    }


    public async Task<string> GetFitbitTokenFromJsonUsingGraph()
    {
        string fileContents = "";
        try
        {
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, FitbitTokenFileName, _tempFilePath);
            fileContents = await File.ReadAllTextAsync(_tempFilePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw;
        }
        return fileContents;
    }

    public async Task UploadTokenDataToGraphAsync(string tokenData)
    {
        try
        {
            await System.IO.File.WriteAllTextAsync(_tempFilePath, tokenData);
            await GraphFileUtility.UploadFile(folderPath, FitbitTokenFileName, _tempFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data to Graph: {ex.Message}");
            throw ex;
        }
    }
}

