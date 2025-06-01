using System;
using System.Collections.Generic;
using System.Text;

namespace Models.DailyLog
{
    public class DailyLog15Min
    {
        public DateTime dtActivity { get; set; }
        public string? activityDesc { get; set; }

        public int colIndex { get; set; }
        public int rowIndex { get; set; }
        public string? category { get; set; }
        public string? activityGroup { get; set; }
        public string? activityName { get; set; }
        public string? activityIndex { get; set; }
        public decimal Hrs { get; set; }
    }
}
