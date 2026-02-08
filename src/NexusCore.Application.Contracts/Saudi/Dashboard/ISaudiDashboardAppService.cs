using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Dashboard;

public interface ISaudiDashboardAppService : IApplicationService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<List<MonthlyInvoiceStatsDto>> GetMonthlyStatsAsync(int? year = null);
    Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
}
