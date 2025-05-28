using System.Globalization;
using System.Text;
using System.IO;
using DailyPulseMVC.Models;

public class PomodoroService
{
    private readonly string _csvPath;

    public PomodoroService()
    {
        _csvPath = "pomodoro.csv";
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

        string line = "test,test,test,test,test,test"; // Default line in case of error
        try
        {
            var startTime = DateTime.Parse(entry.StartTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            var endTime = DateTime.Parse(entry.EndTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            line = $"{entry.Daily15MinLogId},{startTime},{endTime},{entry.Description.Replace(",", ";")}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating file: {ex.Message}");
        }
        File.AppendAllLines(_csvPath, new[] { line }, Encoding.UTF8);
    }

    public List<PomodoroEntry> GetAllEntries()
    {
        var entries = new List<PomodoroEntry>();
        if (!File.Exists(_csvPath)) return entries;

        var lines = File.ReadAllLines(_csvPath);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;

            entries.Add(new PomodoroEntry
            {
                Daily15MinLogId = parts[0],
                StartTime = parts[1],
                EndTime = parts[2],
                Description = parts[3]
            });
        }
        return entries;
    }
}
