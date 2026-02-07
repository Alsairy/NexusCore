using System;
using System.Globalization;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Saudi.HijriCalendar;

public class HijriDateConverter : ITransientDependency
{
    private static readonly UmAlQuraCalendar Calendar = new();

    public HijriDate ToHijri(DateTime gregorianDate)
    {
        return HijriDate.FromGregorian(gregorianDate);
    }

    public DateTime ToGregorian(HijriDate hijriDate)
    {
        return hijriDate.ToGregorian();
    }

    public DateTime ToGregorian(int hijriYear, int hijriMonth, int hijriDay)
    {
        return new HijriDate(hijriYear, hijriMonth, hijriDay).ToGregorian();
    }

    public string FormatHijri(DateTime gregorianDate, string format = "yyyy-MM-dd", string culture = "ar-SA")
    {
        var cultureInfo = new CultureInfo(culture);
        cultureInfo.DateTimeFormat.Calendar = Calendar;
        return gregorianDate.ToString(format, cultureInfo);
    }

    public HijriDate GetToday() => HijriDate.Today();

    public int GetDaysInMonth(int hijriYear, int hijriMonth)
    {
        return HijriDate.GetDaysInMonth(hijriYear, hijriMonth);
    }

    public bool IsValidHijriDate(int year, int month, int day)
    {
        try
        {
            _ = new HijriDate(year, month, day);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
