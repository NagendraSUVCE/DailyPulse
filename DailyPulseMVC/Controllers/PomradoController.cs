using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;

namespace DailyPulseMVC.Controllers;
public class PomodoroController : Controller
{
    private readonly PomodoroService _service = new PomodoroService();

    public ActionResult Timer()
    {
        var entries = _service.GetAllEntries();
        return View("Timer", entries);
    }

    [HttpPost]
    public JsonResult Save([FromBody] PomodoroEntry entry)
    {
        _service.SaveEntry(entry);
        return Json(new { success = true });
    }
}
