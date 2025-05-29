using System.Globalization;
using System.Text;
using System.IO;
using DailyPulseMVC.Models;
using Newtonsoft.Json;

public class PomodoroService
{
    private readonly string _csvPath;

    public PomodoroService()
    {
        _csvPath = "pomodoro.json";
    }

    public void SaveEntry(PomodoroEntry pomodoroEntry)
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
        if (File.Exists(_csvPath))
        {
            var existingJson = File.ReadAllText(_csvPath, Encoding.UTF8);
            if (string.IsNullOrEmpty(existingJson) || existingJson.Trim() == "")
            {
                // If the file is empty or contains only an empty array, initialize entries as an empty list
                Console.WriteLine("Warning: The existing JSON content is null or empty.");
                existingJson = "[]"; // Initialize with an empty JSON array
                entries = new List<PomodoroEntry>();
            }
            else if (existingJson.Trim() == "{}")
            {
                Console.WriteLine("Warning: The existing JSON content is null or empty.");
                existingJson = "[]"; // Initialize with an empty JSON array
                entries = new List<PomodoroEntry>();
            }
            else
            {
                entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PomodoroEntry>>(existingJson);
            }
            
        }

        entries.Add(entry);

        try
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(entries, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_csvPath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to JSON file: {ex.Message}");
        }
    }

    public List<PomodoroEntry> GetAllEntries()
    {
        var entries = new List<PomodoroEntry>();
        if (!File.Exists(_csvPath)) return entries;

        try
        {
            var json = File.ReadAllText(_csvPath, Encoding.UTF8);
            entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PomodoroEntry>>(json) ?? new List<PomodoroEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
        }

        return entries;
    }
}
