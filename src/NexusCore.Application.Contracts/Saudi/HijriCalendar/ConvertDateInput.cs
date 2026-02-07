using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.HijriCalendar;

public class GregorianToHijriInput
{
    [Required]
    public DateTime GregorianDate { get; set; }
}

public class HijriToGregorianInput
{
    [Required]
    [Range(1, 9999)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(1, 30)]
    public int Day { get; set; }
}

public class HijriMonthInfoInput
{
    [Required]
    [Range(1, 9999)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }
}

public class HijriMonthInfoDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? MonthName { get; set; }
    public int DaysInMonth { get; set; }
    public DateTime FirstDayGregorian { get; set; }
    public DateTime LastDayGregorian { get; set; }
}
