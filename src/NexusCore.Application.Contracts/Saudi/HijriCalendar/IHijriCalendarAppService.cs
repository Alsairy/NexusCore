using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.HijriCalendar;

public interface IHijriCalendarAppService : IApplicationService
{
    Task<HijriDateDto> GetTodayAsync();
    Task<HijriDateDto> ConvertToHijriAsync(GregorianToHijriInput input);
    Task<HijriDateDto> ConvertToGregorianAsync(HijriToGregorianInput input);
    Task<HijriMonthInfoDto> GetMonthInfoAsync(HijriMonthInfoInput input);
}
