using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Application service for managing approval delegations.
/// Allows users to delegate their approval authority to other users for a specified period.
/// </summary>
[Authorize(NexusCorePermissions.Workflows.Delegate)]
public class DelegationAppService : NexusCoreAppService, IDelegationAppService
{
    private readonly IRepository<ApprovalDelegation, Guid> _delegationRepository;

    public DelegationAppService(IRepository<ApprovalDelegation, Guid> delegationRepository)
    {
        _delegationRepository = delegationRepository;
    }

    /// <summary>
    /// Gets a paginated list of delegations for the current tenant
    /// </summary>
    public async Task<PagedResultDto<ApprovalDelegationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _delegationRepository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var delegations = await AsyncExecuter.ToListAsync(queryable);

        var dtos = delegations.Select(MapToDto).ToList();

        return new PagedResultDto<ApprovalDelegationDto>(totalCount, dtos);
    }

    /// <summary>
    /// Gets a single delegation by ID
    /// </summary>
    public async Task<ApprovalDelegationDto> GetAsync(Guid id)
    {
        var delegation = await _delegationRepository.GetAsync(id);
        return MapToDto(delegation);
    }

    /// <summary>
    /// Creates a new approval delegation for the current user.
    /// Validates that the delegation period is valid and that the user is not delegating to themselves.
    /// Deactivates any existing active delegations for the current user before creating the new one.
    /// </summary>
    public async Task<ApprovalDelegationDto> CreateAsync(CreateUpdateApprovalDelegationDto input)
    {
        var currentUserId = CurrentUser.GetId();

        ValidateDelegation(currentUserId, input);

        // Deactivate any existing active delegations for this user
        await DeactivateExistingDelegationsAsync(currentUserId);

        var delegation = new ApprovalDelegation(
            GuidGenerator.Create(),
            currentUserId,
            input.DelegateUserId,
            input.StartDate,
            input.EndDate,
            input.Reason,
            CurrentTenant.Id);

        await _delegationRepository.InsertAsync(delegation);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(delegation);
    }

    /// <summary>
    /// Updates an existing approval delegation.
    /// Only the delegator (current user) can update their own delegations.
    /// </summary>
    public async Task<ApprovalDelegationDto> UpdateAsync(Guid id, CreateUpdateApprovalDelegationDto input)
    {
        var currentUserId = CurrentUser.GetId();
        var delegation = await _delegationRepository.GetAsync(id);

        EnsureOwnership(delegation, currentUserId);
        ValidateDelegation(currentUserId, input);

        delegation.DelegateUserId = input.DelegateUserId;
        delegation.StartDate = input.StartDate;
        delegation.EndDate = input.EndDate;
        delegation.Reason = input.Reason;

        await _delegationRepository.UpdateAsync(delegation);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(delegation);
    }

    /// <summary>
    /// Deletes an approval delegation.
    /// Only the delegator (current user) can delete their own delegations.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = CurrentUser.GetId();
        var delegation = await _delegationRepository.GetAsync(id);

        EnsureOwnership(delegation, currentUserId);

        await _delegationRepository.DeleteAsync(delegation);
    }

    /// <summary>
    /// Gets the current user's active delegation (if any).
    /// Returns the most recent active delegation whose date range covers the current time.
    /// </summary>
    public async Task<ApprovalDelegationDto?> GetMyActiveDelegationAsync()
    {
        var currentUserId = CurrentUser.GetId();
        var now = Clock.Now;

        var delegation = await _delegationRepository.FirstOrDefaultAsync(
            d => d.DelegatorUserId == currentUserId
                 && d.IsActive
                 && d.StartDate <= now
                 && d.EndDate >= now);

        return delegation != null ? MapToDto(delegation) : null;
    }

    #region Private Methods

    private static void ValidateDelegation(Guid currentUserId, CreateUpdateApprovalDelegationDto input)
    {
        if (input.DelegateUserId == currentUserId)
        {
            throw new BusinessException("NexusCore:CannotDelegateToSelf");
        }

        if (input.EndDate <= input.StartDate)
        {
            throw new BusinessException("NexusCore:DelegationEndDateMustBeAfterStartDate");
        }
    }

    private static void EnsureOwnership(ApprovalDelegation delegation, Guid currentUserId)
    {
        if (delegation.DelegatorUserId != currentUserId)
        {
            throw new BusinessException("NexusCore:NotOwnerOfDelegation")
                .WithData("DelegationId", delegation.Id);
        }
    }

    private async Task DeactivateExistingDelegationsAsync(Guid delegatorUserId)
    {
        var queryable = await _delegationRepository.GetQueryableAsync();
        var activeDelegations = queryable
            .Where(d => d.DelegatorUserId == delegatorUserId && d.IsActive)
            .ToList();

        foreach (var delegation in activeDelegations)
        {
            delegation.IsActive = false;
            await _delegationRepository.UpdateAsync(delegation);
        }
    }

    private static ApprovalDelegationDto MapToDto(ApprovalDelegation delegation)
    {
        return new ApprovalDelegationDto
        {
            Id = delegation.Id,
            TenantId = delegation.TenantId,
            DelegatorUserId = delegation.DelegatorUserId,
            DelegateUserId = delegation.DelegateUserId,
            StartDate = delegation.StartDate,
            EndDate = delegation.EndDate,
            IsActive = delegation.IsActive,
            Reason = delegation.Reason,
            CreationTime = delegation.CreationTime
        };
    }

    #endregion
}
