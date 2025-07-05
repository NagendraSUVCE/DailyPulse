using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DailyPulseMVC.Models;
using Models.DailyLog;

public class LogSummaryService
{
    public DataTable GetCategorySummary(List<DailyLogSummaryForEachDay> logs, List<int> dayRanges)
    {
        DataTable table = new DataTable();
        table.Columns.Add("CategoryName", typeof(string));
        table.Columns.Add("Conclusion", typeof(string));
        table.Columns.Add("DaysRange", typeof(int));
        table.Columns.Add("PercentageChange", typeof(decimal));

        DateTime today = DateTime.Today;

        var categories = logs.Select(l => l.Category).Distinct();

        foreach (var category in categories)
        {
            var categoryLogs = logs.Where(l => l.Category == category).ToList();

            if (!categoryLogs.Any())
                continue;

            var earliestDate = categoryLogs.Min(l => l.ActivityDate);
            var totalDays = earliestDate.HasValue 
                ? (decimal)(today - earliestDate.Value).TotalDays + 1 
                : 0;
            var overallDailyAverage = categoryLogs.Sum(l => l.TotalValue) / totalDays;

            foreach (var range in dayRanges)
            {
                DateTime rangeStart = today.AddDays(-range);
                DateTime rangeEnd = today.AddDays(-1); // exclude today

                var rangeLogs = categoryLogs
                    .Where(l => l.ActivityDate >= rangeStart && l.ActivityDate <= rangeEnd)
                    .ToList();

                decimal rangeAverage = rangeLogs.Sum(l => l.TotalValue);// range;

                decimal overallRangeAverage = overallDailyAverage * range;

                decimal percentageChange = overallRangeAverage == 0 ? 0 :
                    ((rangeAverage - overallRangeAverage) / overallRangeAverage) * 100;

                string trend = percentageChange > 0 ? "higher" : "lower";

                string conclusion = $"Last {range} day average for {category} is {rangeAverage:F2} and it is {Math.Abs(percentageChange):F0}% {trend} than overall {range}-day average of {overallRangeAverage:F2}";

                table.Rows.Add(category, conclusion, range, percentageChange);
            }
        }

        return table;
    }
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

            var earliestDate = categoryLogs.Min(l => l.ActivityDate);
            var totalDays = earliestDate.HasValue 
                ? (decimal)(today - earliestDate.Value).TotalDays + 1 
                : 0;
            var overallDailyAverage = categoryLogs.Sum(l => l.TotalValue) / totalDays;
            var overallWeeklyAverage = overallDailyAverage * 7;

            var lastWeekLogs = categoryLogs
                .Where(l => l.ActivityDate >= lastWeekStart && l.ActivityDate <= yesterday)
                .ToList();

            decimal lastWeekAverage = lastWeekLogs.Sum(l => l.TotalValue);/// 7;

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
