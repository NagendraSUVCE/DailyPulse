using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DailyPulseMVC.Models;
using Models.DailyLog;

public class LogSummaryService
{
    public DataTable GetCategorySummary(List<DailyLogSummaryForEachDay> logs, List<int> dayRanges , Dictionary<string, decimal> targets)
    {
        dayRanges = dayRanges.Distinct().ToList();
        DataTable table = new DataTable();
        table.Columns.Add("CategoryName", typeof(string));

        // Dynamically add columns for each day range
        foreach (var range in dayRanges)
        {
            table.Columns.Add($"Range_{range}_Days_Summary", typeof(string));
        }

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
            var overallDailyAverage = targets.ContainsKey(category) ? targets[category] : 0;

            // Create a new row for the category
            var row = table.NewRow();
            row["CategoryName"] = category;

            foreach (var range in dayRanges)
            {
            DateTime rangeStart = today.AddDays(-range);
            DateTime rangeEnd = today.AddDays(-1); // exclude today

            var rangeLogs = categoryLogs
                .Where(l => l.ActivityDate >= rangeStart && l.ActivityDate <= rangeEnd)
                .ToList();

            decimal rangeActual = rangeLogs.Sum(l => l.TotalValue);
            decimal rangeTarget = overallDailyAverage * range;

            decimal percentageChange = rangeTarget == 0 ? 0 :
                ((rangeActual - rangeTarget) / rangeTarget) * 100;
                // Determine the cell color based on the percentage change
                string cellColor = percentageChange < 0 
                    ? "red" 
                    : (percentageChange > 10 ? "green" : "black");

                // Combine the values into a single string with conditional formatting for the entire cell
                string summary = $"<span style='color:{cellColor};'>Actual : {rangeActual:F2} ({rangeTarget:F2}) PercDiff : {percentageChange:F2}%</span>";

            // Add the summary to the corresponding column
            row[$"Range_{range}_Days_Summary"] = summary;
            }

            table.Rows.Add(row);
        }

        table.TableName = "CategorySummaryWithDayRanges";
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
