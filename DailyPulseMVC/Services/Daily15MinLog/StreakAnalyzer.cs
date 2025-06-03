using Models.DailyLog;

public static class StreakAnalyzer
{
    public static List<StreakResult> AnalyzeStreaks(List<DailyLogSummaryForEachDay> logs, Dictionary<string, decimal> targets)
    {
        var results = new List<StreakResult>();

        foreach (var category in targets.Keys)
        {
            var target = targets[category];
            var categoryLogs = logs
                .Where(l => l.Category == category && l.ActivityDate.Date < DateTime.Today.Date)
                .OrderByDescending(l => l.ActivityDate)
                .ToList();

            decimal cumulativeSum = 0;
            int count = 0;
            int streak = 0;
            DateTime? streakStart = null;
            DateTime? streakEnd = null;

            for (int i = 0; i < categoryLogs.Count; i++)
            {
                cumulativeSum += categoryLogs[i].TotalHrs;
                count++;
                var avg = cumulativeSum / count;

                if (avg >= target)
                {
                    streak++;
                    if (streakStart == null) streakStart = categoryLogs[i].ActivityDate;
                    streakEnd = categoryLogs[i].ActivityDate;
                }
                else
                {
                    break;
                }
            }

            if (streak > 0 && streakStart.HasValue && streakEnd.HasValue)
            {
                // Calculate required value today to maintain streak
                var requiredToday = (target * (streak + 2)) - cumulativeSum;

                results.Add(new StreakResult
                {
                    AnalyzeDate = DateTime.Now,
                    Category = category,
                    Target = target,
                    StreakDays = streak,
                    StartDate = streakStart.Value,
                    EndDate = streakEnd.Value,
                    RequiredToday = Math.Max(0, requiredToday)
                });
            }
        }

        return results;
    }
}