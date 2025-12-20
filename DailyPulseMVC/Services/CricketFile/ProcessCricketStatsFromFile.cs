/*
https://www.espncricinfo.com/records/trophy/team-series-results/indian-premier-league-117
  var filePathExpensesPending = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/
  CricketFile/CI_AllHtmls/Trophy/Team_Series_Results/Indian_Premier_League_117/";


*/

public class CricketProcessorFromFile
{

  string baseFolderPath = "";
  BattingInningsService battingInningsService = null;
    public CricketProcessorFromFile()
  {
    baseFolderPath = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/CricketFile/CI_AllHtmls/";

     battingInningsService = new BattingInningsService();
  }
  public async Task<List<BattingInnings>> GetBattingInningsFromAllFiles()
  {
    List<BattingInnings> allBattingInnings = new List<BattingInnings>();
    try
    {
      var files = Directory.GetFiles(baseFolderPath, "*batting_innings*", SearchOption.AllDirectories);
      // order by file by name
      files = files.OrderBy(f => Path.GetFileName(f)).ToArray();
      foreach (var file in files)
      {

        // Generate the CSV file path
        string processedFolderPath = baseFolderPath.Replace("CI_AllHtmls", "Processed");
        string relativeFilePath = Path.GetRelativePath(baseFolderPath, file);
        string csvFilePath = Path.Combine(processedFolderPath, Path.ChangeExtension(relativeFilePath, ".csv"));
        if (File.Exists(csvFilePath))
        {
          Console.WriteLine($"CSV file already exists: {csvFilePath}");
          continue; // Skip processing this file
        }
        var innings = await GetBattingInningsFromFile(file);
        // allBattingInnings.AddRange(innings);
        allBattingInnings = innings;
        Console.WriteLine($"[{DateTime.Now}] Successfully processed file: {file}");

        await battingInningsService.SaveBattingInningsToCsv(innings, csvFilePath);

        Console.WriteLine($"CSV file will be saved at: {csvFilePath}");
        // break; // Remove this line to process all files
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[{DateTime.Now}] Error processing files: {ex.Message}");
    }
    return allBattingInnings;
  }

  public async Task<List<BattingInnings>> GetBattingInningsFromFile(string filePath)
  {
    List<BattingInnings> battingInnings = new List<BattingInnings>();
    if (!File.Exists(filePath))
    {
      Console.WriteLine($"File does not exist: {filePath}");
      return battingInnings;
    }


    string htmlContent = File.ReadAllText(filePath);


    battingInnings = await battingInningsService.ParseBattingInningsFromHtml(htmlContent);

    return battingInnings;
  }
}