using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ScheduleSettings
    {
        public bool IsEnabled { get; set; } = true;
        public List<TimeRange> WorkingHours { get; set; } = new();
        public List<DayOfWeek> WorkingDays { get; set; } = new()
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };
    }
}
