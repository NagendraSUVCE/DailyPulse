using System.Globalization;
using System.Text;
using System.IO;
using DailyPulseMVC.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class PomodoroService
{
    // private readonly string _csvPath;
    private readonly string _tempFilePath;
    private string fileName = "pomodoro.json";
    private string folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
    public PomodoroService()
    {
        // _csvPath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/pomodoro.json";
        _tempFilePath = "temp_pomodoro.json";
        fileName = "pomodoro_copy.json";
        folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
    }

    public List<PomodoroEntry> GetAllEntries()
    {
        var entries = new List<PomodoroEntry>();
        try
        {
            // var json = File.ReadAllText(_csvPath, Encoding.UTF8);
            var json = GetPomodoroDetailsFromJsonUsingGraph().Result;
            entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PomodoroEntry>>(json) ?? new List<PomodoroEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
        }

        return entries;
    }

    public async Task SaveEntry(PomodoroEntry pomodoroEntry)
    {
        var entry = pomodoroEntry;
        if (entry == null)
        {
            Console.WriteLine("Error: The provided PomodoroEntry is null.");
            return;
        }

        if (string.IsNullOrEmpty(entry.Daily15MinLogId) || string.IsNullOrEmpty(entry.StartTime) || string.IsNullOrEmpty(entry.EndTime) || string.IsNullOrEmpty(entry.Description))
        {
            Console.WriteLine("Error: One or more properties of PomodoroEntry are null or empty.");
            return;
        }

        var entries = new List<PomodoroEntry>();
        entries = GetAllEntries();
        entries.Add(entry);

        try
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(entries, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_tempFilePath, json, Encoding.UTF8);
            await UploadDataToGraphAsync(_tempFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to JSON file: {ex.Message}");
        }
    }

    public async Task<string> GetPomodoroDetailsFromJsonUsingGraph()
    {

        string fileContents = "";

        try
        {
            await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, _tempFilePath);
            fileContents = await File.ReadAllTextAsync(_tempFilePath, Encoding.UTF8);

        }
        catch (Exception ex)
        {
            throw;
        }
        return fileContents;
    }

    public async Task UploadDataToGraphAsync(string filePath)
    {
        var fileName = "pomodoro_copy.json";
        var folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
        string fileContents = "";
        try
        {
            await GraphFileUtility.UploadFile(folderPath, fileName, _tempFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data to Graph: {ex.Message}");
        }
    }
}
