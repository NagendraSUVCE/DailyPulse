using System.Globalization;
using System.Text;
using System.IO;
using DailyPulseMVC.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class PomodoroService
{
    private readonly string _csvPath;
    private readonly string _tempFilePath;
    private string fileName = "pomodoro.json";
    private string folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
    public PomodoroService()
    {
        _csvPath = @"/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/Nagendra/SelfCode/DatabaseInCSV/pomodoro.json";
        _tempFilePath = "temp_pomodoro.json";
        fileName = "pomodoro.json";
        folderPath = @"Nagendra/SelfCode/DatabaseInCSV";
    }

    public List<PomodoroEntry> GetAllEntries()
    {
        var entries = new List<PomodoroEntry>();
        try
        {
            string jsonFromGraph = GetPomodoroDetailsFromJsonUsingGraph().Result;
            entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PomodoroEntry>>(jsonFromGraph) ?? new List<PomodoroEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
            string jsonFromLocalFile = File.ReadAllText(_csvPath, Encoding.UTF8);
            entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PomodoroEntry>>(jsonFromLocalFile) ?? new List<PomodoroEntry>();
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
        var jsonFromFile = Newtonsoft.Json.JsonConvert.SerializeObject(entries, Newtonsoft.Json.Formatting.Indented);

        try
        {
            File.WriteAllText(_tempFilePath, jsonFromFile, Encoding.UTF8);
            UploadDataToGraphAsync(_tempFilePath).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to JSON file: {ex.Message}");
            File.WriteAllText(_csvPath, jsonFromFile, Encoding.UTF8);
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
        try
        {
            await GraphFileUtility.UploadFile(folderPath, fileName, _tempFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data to Graph: {ex.Message}");
            throw ex;
        }
    }
}
