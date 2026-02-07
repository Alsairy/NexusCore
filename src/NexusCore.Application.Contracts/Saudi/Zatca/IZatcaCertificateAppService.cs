using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Zatca;

public interface IZatcaCertificateAppService : IApplicationService
{
    Task<PagedResultDto<ZatcaCertificateDto>> GetListAsync(Guid sellerId, PagedAndSortedResultRequestDto input);

    Task<ZatcaCertificateDto> GetAsync(Guid id);

    Task<ZatcaCertificateDto> CreateAsync(CreateZatcaCertificateDto input);

    Task DeleteAsync(Guid id);

    Task ActivateAsync(Guid id);

    Task DeactivateAsync(Guid id);
}
