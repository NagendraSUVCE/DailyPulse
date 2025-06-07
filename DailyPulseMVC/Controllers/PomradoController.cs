using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using System.Threading.Tasks;

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
    public async Task<JsonResult> Save([FromBody] PomodoroEntry entry)
    {
        await _service.SaveEntry(entry);
        return Json(new { success = true });
    }
}
