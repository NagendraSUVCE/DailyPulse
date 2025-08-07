using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DailyPulseMVC.Services.Diary;

public class DiaryController : Controller
{

    public async Task<IActionResult> Index()
    {
        var diaryServiceObj = new DiaryService();
        var entries = await diaryServiceObj.LoadEntries();
        return View(entries);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(DiaryEntry entry)
    {
        var diaryServiceObj = new DiaryService();
        entry.LoggedTime = DateTime.Now;
        var entries = await diaryServiceObj.LoadEntries();
        entries.Add(entry);
        await diaryServiceObj.SaveEntries(entries);
        return RedirectToAction("Index");
    }
}
