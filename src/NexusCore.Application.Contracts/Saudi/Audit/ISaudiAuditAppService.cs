using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Audit;

public interface ISaudiAuditAppService : IApplicationService
{
    Task<PagedResultDto<SaudiAuditLogDto>> GetListAsync(GetSaudiAuditListInput input);
    Task<SaudiAuditLogDto> GetAsync(System.Guid id);
    Task<List<SaudiEntityChangeDto>> GetEntityHistoryAsync(string entityType, string entityId);
}
