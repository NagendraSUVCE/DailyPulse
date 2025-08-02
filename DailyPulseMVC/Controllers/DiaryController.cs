using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class DiaryController : Controller
{
    private readonly string _filePath = "App_Data/diary.json";

    public IActionResult Index()
    {
        var entries = LoadEntries();
        return View(entries);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(DiaryEntry entry)
    {
        entry.LoggedTime = DateTime.Now;
        var entries = LoadEntries();
        entries.Add(entry);
        SaveEntries(entries);
        return RedirectToAction("Index");
    }

    private List<DiaryEntry> LoadEntries()
    {
        if (!System.IO.File.Exists(_filePath))
            return new List<DiaryEntry>();
            var json = System.IO.File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<DiaryEntry>();
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json)?.OrderByDescending(entry => entry.LoggedTime).ToList() ?? new List<DiaryEntry>();
    }

    private void SaveEntries(List<DiaryEntry> entries)
    {
        var json = JsonSerializer.Serialize(entries);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        System.IO.File.WriteAllText(_filePath, json);
    }
}
