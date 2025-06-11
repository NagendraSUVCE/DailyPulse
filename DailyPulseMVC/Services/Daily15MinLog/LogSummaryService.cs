using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DailyPulseMVC.Models;
using Models.DailyLog;

public class LogSummaryService
{
    public DataTable GetCategoryWeeklySummary(List<DailyLogSummaryForEachDay> logs)
    {
        DataTable table = new DataTable();
        table.Columns.Add("CategoryName", typeof(string));
        table.Columns.Add("Conclusion", typeof(string));

        DateTime today = DateTime.Today;
        DateTime yesterday = today.AddDays(-1);
        DateTime lastWeekStart = yesterday.AddDays(-6);

        var categories = logs.Select(l => l.Category).Distinct();

        foreach (var category in categories)
        {
            var categoryLogs = logs.Where(l => l.Category == category).ToList();

            if (!categoryLogs.Any())
                continue;

            var totalDays = (decimal)(today - categoryLogs.Min(l => l.ActivityDate)).TotalDays + 1;
            var overallDailyAverage = categoryLogs.Sum(l => l.TotalHrs) / totalDays;
            var overallWeeklyAverage = overallDailyAverage * 7;

            var lastWeekLogs = categoryLogs
                .Where(l => l.ActivityDate >= lastWeekStart && l.ActivityDate <= yesterday)
                .ToList();

            decimal lastWeekAverage = lastWeekLogs.Sum(l => l.TotalHrs);/// 7;

            decimal percentageChange = overallWeeklyAverage == 0 ? 0 :
                ((lastWeekAverage - overallWeeklyAverage) / overallWeeklyAverage) * 100;

            string trend = percentageChange > 0 ? "higher" : "lower";

            string conclusion = $"Last one week for {category} is {lastWeekAverage:F2} and it is {Math.Abs(percentageChange):F0}% {trend} than overall weekly average of {overallWeeklyAverage:F2}";

            table.Rows.Add(category, conclusion);
        }
        table.TableName = "CategoryWeeklySummaryComparision";
        return table;
    }
}
