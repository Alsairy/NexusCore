using System.Threading.Tasks;
using NexusCore.Saudi.HijriCalendar;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.HijriCalendar;

public class HijriCalendarAppService : ApplicationService, IHijriCalendarAppService
{
    private readonly HijriDateConverter _converter;

    public HijriCalendarAppService(HijriDateConverter converter)
    {
        _converter = converter;
    }

    public Task<HijriDateDto> GetTodayAsync()
    {
        var hijri = _converter.GetToday();
        return Task.FromResult(MapToDto(hijri));
    }

    public Task<HijriDateDto> ConvertToHijriAsync(GregorianToHijriInput input)
    {
        var hijri = _converter.ToHijri(input.GregorianDate);
        return Task.FromResult(MapToDto(hijri));
    }

    public Task<HijriDateDto> ConvertToGregorianAsync(HijriToGregorianInput input)
    {
        var hijri = new HijriDate(input.Year, input.Month, input.Day);
        return Task.FromResult(MapToDto(hijri));
    }

    public Task<HijriMonthInfoDto> GetMonthInfoAsync(HijriMonthInfoInput input)
    {
        var daysInMonth = _converter.GetDaysInMonth(input.Year, input.Month);
        var firstDay = new HijriDate(input.Year, input.Month, 1);
        var lastDay = new HijriDate(input.Year, input.Month, daysInMonth);

        return Task.FromResult(new HijriMonthInfoDto
        {
            Year = input.Year,
            Month = input.Month,
            MonthName = firstDay.GetMonthName(),
            DaysInMonth = daysInMonth,
            FirstDayGregorian = firstDay.ToGregorian(),
            LastDayGregorian = lastDay.ToGregorian()
        });
    }

    private static HijriDateDto MapToDto(HijriDate hijri)
    {
        return new HijriDateDto
        {
            Year = hijri.Year,
            Month = hijri.Month,
            Day = hijri.Day,
            MonthName = hijri.GetMonthName(),
            DayOfWeekName = hijri.GetDayOfWeekName(),
            Formatted = hijri.ToDisplayString(),
            GregorianDate = hijri.ToGregorian()
        };
    }
}
