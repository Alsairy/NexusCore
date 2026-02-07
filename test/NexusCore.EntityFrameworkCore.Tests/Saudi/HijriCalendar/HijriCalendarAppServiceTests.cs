using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.HijriCalendar;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.HijriCalendar;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class HijriCalendarAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IHijriCalendarAppService _hijriCalendarAppService;

    public HijriCalendarAppServiceTests()
    {
        _hijriCalendarAppService = GetRequiredService<IHijriCalendarAppService>();
    }

    [Fact]
    public async Task GetToday_Should_Return_Valid_HijriDate()
    {
        var result = await _hijriCalendarAppService.GetTodayAsync();

        result.ShouldNotBeNull();
        result.Year.ShouldBeGreaterThan(1400);
        result.Month.ShouldBeGreaterThanOrEqualTo(1);
        result.Month.ShouldBeLessThanOrEqualTo(12);
        result.Day.ShouldBeGreaterThanOrEqualTo(1);
        result.Day.ShouldBeLessThanOrEqualTo(30);
        result.MonthName.ShouldNotBeNullOrWhiteSpace();
        result.DayOfWeekName.ShouldNotBeNullOrWhiteSpace();
        result.Formatted.ShouldNotBeNullOrWhiteSpace();
        result.GregorianDate.Date.ShouldBe(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task ConvertToHijri_Known_Date_Should_Return_Correct_Values()
    {
        // 2024-01-15 is a known Gregorian date
        var result = await _hijriCalendarAppService.ConvertToHijriAsync(
            new GregorianToHijriInput { GregorianDate = new DateTime(2024, 1, 15) });

        result.ShouldNotBeNull();
        result.Year.ShouldBeGreaterThan(1400);
        result.Month.ShouldBeGreaterThanOrEqualTo(1);
        result.Month.ShouldBeLessThanOrEqualTo(12);
        result.Day.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ConvertToGregorian_RoundTrip_Should_Return_Same_Date()
    {
        var gregorianDate = new DateTime(2024, 6, 15);

        // Convert to Hijri
        var hijriResult = await _hijriCalendarAppService.ConvertToHijriAsync(
            new GregorianToHijriInput { GregorianDate = gregorianDate });

        // Convert back to Gregorian
        var gregorianResult = await _hijriCalendarAppService.ConvertToGregorianAsync(
            new HijriToGregorianInput
            {
                Year = hijriResult.Year,
                Month = hijriResult.Month,
                Day = hijriResult.Day
            });

        gregorianResult.GregorianDate.Date.ShouldBe(gregorianDate.Date);
    }

    [Fact]
    public async Task GetMonthInfo_Should_Return_Valid_MonthInfo()
    {
        var today = HijriDate.Today();

        var result = await _hijriCalendarAppService.GetMonthInfoAsync(
            new HijriMonthInfoInput { Year = today.Year, Month = today.Month });

        result.ShouldNotBeNull();
        result.Year.ShouldBe(today.Year);
        result.Month.ShouldBe(today.Month);
        result.MonthName.ShouldNotBeNullOrWhiteSpace();
        result.DaysInMonth.ShouldBeGreaterThanOrEqualTo(29);
        result.DaysInMonth.ShouldBeLessThanOrEqualTo(30);
        result.FirstDayGregorian.ShouldBeLessThan(result.LastDayGregorian);
    }
}
