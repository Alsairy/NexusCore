using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Application service interface for managing approval delegations.
/// Allows users to delegate their approval authority to other users for a specified period.
/// </summary>
public interface IDelegationAppService : IApplicationService
{
    /// <summary>
    /// Gets a paginated list of delegations for the current tenant
    /// </summary>
    /// <param name="input">Paging and sorting parameters</param>
    /// <returns>Paged list of approval delegation DTOs</returns>
    Task<PagedResultDto<ApprovalDelegationDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Gets a single delegation by ID
    /// </summary>
    /// <param name="id">The delegation ID</param>
    /// <returns>The approval delegation DTO</returns>
    Task<ApprovalDelegationDto> GetAsync(Guid id);

    /// <summary>
    /// Creates a new approval delegation for the current user
    /// </summary>
    /// <param name="input">The delegation details</param>
    /// <returns>The created approval delegation DTO</returns>
    Task<ApprovalDelegationDto> CreateAsync(CreateUpdateApprovalDelegationDto input);

    /// <summary>
    /// Updates an existing approval delegation
    /// </summary>
    /// <param name="id">The delegation ID</param>
    /// <param name="input">The updated delegation details</param>
    /// <returns>The updated approval delegation DTO</returns>
    Task<ApprovalDelegationDto> UpdateAsync(Guid id, CreateUpdateApprovalDelegationDto input);

    /// <summary>
    /// Deletes an approval delegation
    /// </summary>
    /// <param name="id">The delegation ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets the current user's active delegation (if any)
    /// </summary>
    /// <returns>The active delegation DTO, or null if none exists</returns>
    Task<ApprovalDelegationDto?> GetMyActiveDelegationAsync();
}
