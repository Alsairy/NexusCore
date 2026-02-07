using System;
using NexusCore.Saudi.HijriCalendar;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.HijriCalendar;

public class HijriDateTests
{
    [Fact]
    public void Should_Create_Via_FromGregorian()
    {
        var gregorianDate = new DateTime(2024, 6, 15);
        var date = HijriDate.FromGregorian(gregorianDate);

        date.Year.ShouldBeGreaterThan(0);
        date.Month.ShouldBeGreaterThanOrEqualTo(1);
        date.Month.ShouldBeLessThanOrEqualTo(12);
        date.Day.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Should_Have_Value_Equality()
    {
        var gregorianDate = new DateTime(2024, 6, 15);
        var date1 = HijriDate.FromGregorian(gregorianDate);
        var date2 = HijriDate.FromGregorian(gregorianDate);

        date1.ShouldBe(date2);
        (date1 == date2).ShouldBeTrue();
        date1.GetHashCode().ShouldBe(date2.GetHashCode());
    }

    [Fact]
    public void Should_Not_Be_Equal_For_Different_Dates()
    {
        var date1 = HijriDate.FromGregorian(new DateTime(2024, 6, 15));
        var date2 = HijriDate.FromGregorian(new DateTime(2024, 6, 16));

        date1.ShouldNotBe(date2);
        (date1 != date2).ShouldBeTrue();
    }

    [Fact]
    public void Should_Compare_Correctly()
    {
        var earlier = HijriDate.FromGregorian(new DateTime(2024, 1, 1));
        var later = HijriDate.FromGregorian(new DateTime(2024, 6, 15));

        (earlier < later).ShouldBeTrue();
        (later > earlier).ShouldBeTrue();
        (earlier <= later).ShouldBeTrue();
        (later >= earlier).ShouldBeTrue();
    }

    [Fact]
    public void Should_RoundTrip_FromGregorian_ToGregorian()
    {
        var gregorianDate = new DateTime(2024, 1, 15);
        var hijriDate = HijriDate.FromGregorian(gregorianDate);
        var roundTripped = hijriDate.ToGregorian();

        roundTripped.Year.ShouldBe(gregorianDate.Year);
        roundTripped.Month.ShouldBe(gregorianDate.Month);
        roundTripped.Day.ShouldBe(gregorianDate.Day);
    }

    [Fact]
    public void Today_Should_Return_Valid_Date()
    {
        var today = HijriDate.Today();

        today.Year.ShouldBeGreaterThan(1400);
        today.Month.ShouldBeGreaterThanOrEqualTo(1);
        today.Month.ShouldBeLessThanOrEqualTo(12);
        today.Day.ShouldBeGreaterThanOrEqualTo(1);
        today.Day.ShouldBeLessThanOrEqualTo(30);
    }

    [Fact]
    public void Should_Throw_For_Invalid_Month()
    {
        var today = HijriDate.Today();
        Should.Throw<ArgumentException>(() =>
            new HijriDate(today.Year, 13, 1));
    }

    [Fact]
    public void Should_Throw_For_Invalid_Day()
    {
        var today = HijriDate.Today();
        Should.Throw<ArgumentException>(() =>
            new HijriDate(today.Year, today.Month, 31));
    }

    [Fact]
    public void GetMonthName_Should_Return_Name()
    {
        var date = HijriDate.FromGregorian(new DateTime(2024, 1, 15));
        var monthName = date.GetMonthName("ar-SA");

        monthName.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ToString_Should_Return_Formatted_String()
    {
        var date = HijriDate.FromGregorian(new DateTime(2024, 1, 15));
        var str = date.ToString();

        str.ShouldNotBeNullOrWhiteSpace();
        str.ShouldMatch(@"^\d{4}-\d{2}-\d{2}$");
    }

    [Fact]
    public void GetDaysInMonth_Should_Return_Valid_Count()
    {
        var today = HijriDate.Today();
        var days = HijriDate.GetDaysInMonth(today.Year, 1);
        days.ShouldBeGreaterThanOrEqualTo(29);
        days.ShouldBeLessThanOrEqualTo(30);
    }

    [Fact]
    public void Null_Equality_Should_Work()
    {
        var date = HijriDate.FromGregorian(new DateTime(2024, 1, 15));
        (date == null).ShouldBeFalse();
        (null == date).ShouldBeFalse();

        HijriDate? nullDate = null;
        (nullDate == null).ShouldBeTrue();
    }
}
