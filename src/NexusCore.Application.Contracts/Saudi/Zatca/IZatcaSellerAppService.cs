using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Application service interface for ZATCA Seller management
/// </summary>
public interface IZatcaSellerAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of sellers
    /// </summary>
    Task<PagedResultDto<ZatcaSellerDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get a single seller by ID
    /// </summary>
    Task<ZatcaSellerDto> GetAsync(Guid id);

    /// <summary>
    /// Create a new seller
    /// </summary>
    Task<ZatcaSellerDto> CreateAsync(CreateUpdateZatcaSellerDto input);

    /// <summary>
    /// Update an existing seller
    /// </summary>
    Task<ZatcaSellerDto> UpdateAsync(Guid id, CreateUpdateZatcaSellerDto input);

    /// <summary>
    /// Delete a seller
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Set a seller as the default seller (deactivates all others)
    /// </summary>
    Task SetDefaultAsync(Guid id);

    /// <summary>
    /// Get the default seller
    /// </summary>
    Task<ZatcaSellerDto?> GetDefaultAsync();
}
