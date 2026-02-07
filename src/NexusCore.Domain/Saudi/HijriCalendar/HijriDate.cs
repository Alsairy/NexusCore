using System;
using System.Globalization;
using Volo.Abp;

namespace NexusCore.Saudi.HijriCalendar;

public sealed class HijriDate : IEquatable<HijriDate>, IComparable<HijriDate>
{
    private static readonly UmAlQuraCalendar Calendar = new();
    private static readonly int MinHijriYear = Calendar.GetYear(Calendar.MinSupportedDateTime);
    private static readonly int MaxHijriYear = Calendar.GetYear(Calendar.MaxSupportedDateTime);

    public int Year { get; }
    public int Month { get; }
    public int Day { get; }

    public HijriDate(int year, int month, int day)
    {
        Check.Range(year, nameof(year), MinHijriYear, MaxHijriYear);
        Check.Range(month, nameof(month), 1, 12);
        Check.Range(day, nameof(day), 1, Calendar.GetDaysInMonth(year, month));

        Year = year;
        Month = month;
        Day = day;
    }

    public DateTime ToGregorian()
    {
        return Calendar.ToDateTime(Year, Month, Day, 0, 0, 0, 0);
    }

    public static HijriDate FromGregorian(DateTime gregorianDate)
    {
        var year = Calendar.GetYear(gregorianDate);
        var month = Calendar.GetMonth(gregorianDate);
        var day = Calendar.GetDayOfMonth(gregorianDate);
        return new HijriDate(year, month, day);
    }

    public static HijriDate Today()
    {
        return FromGregorian(DateTime.UtcNow);
    }

    public string GetMonthName(string culture = "ar-SA")
    {
        var cultureInfo = new CultureInfo(culture);
        cultureInfo.DateTimeFormat.Calendar = Calendar;
        var dt = ToGregorian();
        return dt.ToString("MMMM", cultureInfo);
    }

    public string GetDayOfWeekName(string culture = "ar-SA")
    {
        var cultureInfo = new CultureInfo(culture);
        cultureInfo.DateTimeFormat.Calendar = Calendar;
        var dt = ToGregorian();
        return dt.ToString("dddd", cultureInfo);
    }

    public static int GetDaysInMonth(int year, int month) => Calendar.GetDaysInMonth(year, month);

    public static int GetMonthsInYear(int year) => Calendar.GetMonthsInYear(year);

    public override string ToString() => $"{Year:D4}-{Month:D2}-{Day:D2}";

    public string ToDisplayString(string culture = "ar-SA")
    {
        var cultureInfo = new CultureInfo(culture);
        cultureInfo.DateTimeFormat.Calendar = Calendar;
        var dt = ToGregorian();
        return dt.ToString("dd MMMM yyyy", cultureInfo);
    }

    public bool Equals(HijriDate? other)
    {
        if (other is null) return false;
        return Year == other.Year && Month == other.Month && Day == other.Day;
    }

    public override bool Equals(object? obj) => Equals(obj as HijriDate);

    public override int GetHashCode() => HashCode.Combine(Year, Month, Day);

    public int CompareTo(HijriDate? other)
    {
        if (other is null) return 1;
        var cmp = Year.CompareTo(other.Year);
        if (cmp != 0) return cmp;
        cmp = Month.CompareTo(other.Month);
        return cmp != 0 ? cmp : Day.CompareTo(other.Day);
    }

    public static bool operator ==(HijriDate? left, HijriDate? right) =>
        left is null ? right is null : left.Equals(right);
    public static bool operator !=(HijriDate? left, HijriDate? right) => !(left == right);
    public static bool operator <(HijriDate left, HijriDate right) => left.CompareTo(right) < 0;
    public static bool operator >(HijriDate left, HijriDate right) => left.CompareTo(right) > 0;
    public static bool operator <=(HijriDate left, HijriDate right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HijriDate left, HijriDate right) => left.CompareTo(right) >= 0;
}
