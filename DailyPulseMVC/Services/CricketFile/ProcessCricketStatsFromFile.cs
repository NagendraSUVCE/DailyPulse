/*
https://www.espncricinfo.com/records/trophy/team-series-results/indian-premier-league-117
  var filePathExpensesPending = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/
  CricketFile/CI_AllHtmls/Trophy/Team_Series_Results/Indian_Premier_League_117/";


*/

public class CricketProcessorFromfile
{

  string baseFolderPath = "";
  public CricketProcessorFromfile()
  {
baseFolderPath = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/CricketFile/CI_AllHtmls/";
  }
  public async Task<List<BattingInnings>> GetBattingInningsFromAllFiles()
  {
    List<BattingInnings> allBattingInnings = new List<BattingInnings>();
    try
    {
      var files = Directory.GetFiles(baseFolderPath, "*batting_innings*", SearchOption.AllDirectories);
      foreach (var file in files)
      {
        var innings = await GetBattingInningsFromFile(file);
        allBattingInnings.AddRange(innings);
        Console.WriteLine($"[{DateTime.Now}] Successfully processed file: {file}");
        break; // Remove this line to process all files
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
    BattingInningsService battingInningsService = new BattingInningsService();
    battingInnings = await battingInningsService.ParseBattingInningsFromHtml(htmlContent);
    return battingInnings;
  }
}