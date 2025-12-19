using DailyPulseMVC.Services.CricketFile;

public interface IInningsService
{
    public string Url { get; set; }
    public int PageNumber { get; set; }
}

public class InningsService
{
    private readonly DownloadWebService _downloadWebService;
    public List<CricinfoPagination> cricinfoPaginations { get; set; }
    string baseFolderPath = "";
    public InningsService()
    {
        _downloadWebService = new DownloadWebService();
        baseFolderPath = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/CricketFile/CI_AllHtmls/";
    }

    public async Task GetAllHtmls()
    {
        foreach (var pagination in cricinfoPaginations)
        {
            for (int i = 1; i <= pagination.TotalPages; i++)
            {
                string url = GetUrl(pagination, i); //$"{pagination.CricinfoStatsUrl};page={i}";
                string completeFilePath = GetFileName(pagination, i);

                if (i <= pagination.TotalPages - 2 && File.Exists(completeFilePath))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Skipped downloading as file already exists: {completeFilePath}");
                    continue;
                }
                try
                {
                    await SaveUrlContentToFileAsync(url, completeFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error downloading URL: {url}. Exception: {ex.Message}");
                    continue;
                }
            }
        }
    }
    public async Task GetAllHtmlsParallel()
    {
        foreach (var pagination in cricinfoPaginations)
        {
            await Task.WhenAll(Enumerable.Range(1, pagination.TotalPages).Select(async i =>
            {
                string url = GetUrl(pagination, i);
                string completeFilePath = GetFileName(pagination, i);

                if (i <= pagination.TotalPages - 2 && File.Exists(completeFilePath))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Skipped downloading as file already exists: {completeFilePath}");
                    return;
                }
                try
                {
                    await SaveUrlContentToFileAsync(url, completeFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error downloading URL: {url}. Exception: {ex.Message}");
                }
            }));
        }
    }
    private string GetUrl(CricinfoPagination cricinfoPagination, int iterationCount)
    {
        return $"{cricinfoPagination.CricinfoStatsUrl};page={iterationCount}";
    }
    private string GetFileName(CricinfoPagination pag, int iterationCount)
    {
        string folderPath = Path.Combine(baseFolderPath, pag.CricketClassDesc, pag.CricketType, pag.CricketView);
        Directory.CreateDirectory(folderPath);
        string fileName = $"{pag.CricketClassDesc}_{pag.CricketType}_{pag.CricketView}_Page_{iterationCount.ToString().PadLeft(4, '0')}.html";
        string completeFilePath = Path.Combine(folderPath, fileName);
        return completeFilePath;

    }
    public async Task SaveUrlContentToFileAsync(string url, string fileName)
    {
        string filePath = Path.Combine(baseFolderPath, fileName);
        var htmlContent = await _downloadWebService.DownloadContentAsync(url);
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            await writer.WriteAsync(htmlContent);
        }
    }
}