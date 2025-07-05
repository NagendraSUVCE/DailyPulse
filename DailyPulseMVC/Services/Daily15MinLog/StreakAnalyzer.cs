using Models.DailyLog;

public static class StreakAnalyzer
{
    public static List<StreakResult> AnalyzeStreaks(List<DailyLogSummaryForEachDay> logs, Dictionary<string, decimal> targets)
    {
        var results = new List<StreakResult>();
        string streakString = string.Empty;
        foreach (var category in targets.Keys)
        {
            var target = targets[category];
            var categoryLogs = logs
                .Where(l => l.Category == category && l.ActivityDate?.Date < DateTime.Today.Date)
                .OrderByDescending(l => l.ActivityDate)
                .ToList();

            decimal cumulativeSum = 0;
            int count = 0;
            int streak = 0;
            DateTime? streakStart = null;
            DateTime? streakEnd = null;

            for (int i = 0; i < categoryLogs.Count; i++)
            {
                cumulativeSum += categoryLogs[i].TotalValue;
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
                    streak++;
                    if (streakStart == null) streakStart = categoryLogs[i].ActivityDate;
                    streakEnd = categoryLogs[i].ActivityDate;
                    break;
                }
            }

            if (streak > 0 && streakStart.HasValue && streakEnd.HasValue)
            {
                // Calculate required value today to maintain streak
                var requiredToday = (target * (streak + 1)) - cumulativeSum;

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
        foreach (var result in results)
        {
            streakString = $"You met continuous target of {targets[result.Category]} for {result.StreakDays} days " +
                              $"starting from {result.EndDate:dd MMM yyyy} till {result.StartDate:dd MMM yyyy} " +
                              $"and you need {result.RequiredToday:F2} today to increase streak to {result.StreakDays + 1} days.";
            result.StreakString = streakString;
        }
        return results;
    }
}