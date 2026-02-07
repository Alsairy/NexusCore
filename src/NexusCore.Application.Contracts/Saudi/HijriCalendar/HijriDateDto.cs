using System;

namespace NexusCore.Saudi.HijriCalendar;

public class HijriDateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }

    public string? MonthName { get; set; }
    public string? DayOfWeekName { get; set; }
    public string? Formatted { get; set; }

    public DateTime GregorianDate { get; set; }
}
