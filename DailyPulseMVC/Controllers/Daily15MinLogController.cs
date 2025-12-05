using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using DailyPulseMVC.Models;
using Models.DailyLog;
using Models.Finance;
using System.Threading.Tasks;

namespace DailyPulseMVC.Controllers;

public class Daily15MinLogController : Controller
{
    // dotnet publish -c Release -o ./bin/Publish
    private readonly ILogger<Daily15MinLogController> _logger;

    public Daily15MinLogController(ILogger<Daily15MinLogController> logger)
    {
        _logger = logger;
    }


    public async Task<IActionResult> Index()
    {
        var lstDailyLog15Min = await (new Daily15MinLogService()).GetDaily15MinLogAsyncForYear2025();
        return View(lstDailyLog15Min);
    }

    public async Task<IActionResult> Common()
    {
        DataSet dataSet = (new ExpensesService()).GetPayslipsSummarizedGraphWay().Result;
        DataTable dataTable1 = dataSet.Tables[0];
        foreach (DataTable table in dataSet.Tables)
        {
            if (table.TableName == "Expenses2022")
            {
                dataTable1 = table;
                break;
            }
        }
        DataSet dsNew = new DataSet();
        dsNew.Tables.Add(dataTable1.Copy());
        return await Task.FromResult(View(dsNew));
    }

    public IActionResult AvgStreak(bool saveInCSV = false)
    {
        DataSet dsNew = new DataSet();
        dsNew = (new Daily15MinLogService()).AvgStreak(saveInCSV).Result;

        return View("Common", dsNew);
    }

    public async Task<IActionResult> DailyLogAvgStreakSendEmail()
    {
        DataSet dsNew = new DataSet();
        dsNew = (new Daily15MinLogService()).AvgStreak(false).Result;
        string emailBody = "<html><body>";
        emailBody += "<h2>Daily Log Average Streak Data</h2>";
        emailBody += "<table border='1' style='border-collapse: collapse; width: 100%;'>";
        emailBody += "<thead><tr style='background-color: #f2f2f2;'>";
        string columnName = "";
        foreach (DataTable table in dsNew.Tables)
        {
            emailBody += $"<th colspan='{table.Columns.Count}' style='text-align: center;'>{table.TableName}</th></tr><tr>";

            foreach (DataColumn column in table.Columns)
            {
                columnName = column.ColumnName;
                if (columnName.Contains("Days_Summary"))
                {
                    columnName = columnName.Replace("Days_Summary", " Days Summary");
                }
                emailBody += $"<th style='padding: 8px; text-align: left;'>{columnName}</th>";
            }
            emailBody += "</tr></thead><tbody>";

            foreach (DataRow row in table.Rows)
            {
                emailBody += "<tr>";
                foreach (var item in row.ItemArray)
                {
                    emailBody += $"<td style='padding: 8px; text-align: left;'>{item}</td>";
                }
                emailBody += "</tr>";
            }
            emailBody += "</tbody>";
        }
        emailBody += "</table>";
        emailBody += "</body></html>";
        EmailService emailService = new EmailService();
        try
        {
            emailService.SendEmail("nagendra.uvce@gmail.com", "nagendra_s_uvce@hotmail.com", emailBody, "Last one week Data");
            emailService.SendEmail("nagendra.uvce@gmail.com", "nagendra_subramanya@optum.com", emailBody, "Last one week Data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return Ok("Email Sendt Failed" + emailBody + ex.Message);
        }
        return Ok("Email Sent");
    }

    public IActionResult Charts()
    {
        List<DailyLogSummaryForEachDay> lstDailyLogSummary = (new Daily15MinLogService()).GetDailyLogSummaryForEachDaysAsync().Result;

        return View("Charts", lstDailyLogSummary);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
