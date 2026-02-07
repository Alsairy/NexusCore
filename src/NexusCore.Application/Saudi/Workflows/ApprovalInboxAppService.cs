using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using NexusCore.Saudi.Workflows.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Application service for the approval inbox.
/// Provides operations for viewing and acting on approval tasks assigned to the current user.
/// Uses the ApprovalWorkflowManager domain service for core business logic.
/// </summary>
[Authorize(NexusCorePermissions.Workflows.Approve)]
public class ApprovalInboxAppService : NexusCoreAppService, IApprovalInboxAppService
{
    private readonly ApprovalWorkflowManager _workflowManager;
    private readonly IRepository<ApprovalTask, Guid> _taskRepository;

    public ApprovalInboxAppService(
        ApprovalWorkflowManager workflowManager,
        IRepository<ApprovalTask, Guid> taskRepository)
    {
        _workflowManager = workflowManager;
        _taskRepository = taskRepository;
    }

    /// <summary>
    /// Gets the paginated list of approval tasks assigned to the current user,
    /// including tasks delegated to the current user by other users.
    /// Supports filtering by status and date range, with paging and sorting.
    /// </summary>
    public async Task<PagedResultDto<ApprovalTaskDto>> GetMyTasksAsync(GetApprovalTaskListInput input)
    {
        var userId = CurrentUser.GetId();

        // Get all pending tasks (including delegated) from the domain service
        var allTasks = await _workflowManager.GetPendingTasksForUserAsync(userId);
        var taskIds = allTasks.Select(t => t.Id).ToList();

        // Build queryable for filtering and paging over the full task set
        var queryable = await _taskRepository.GetQueryableAsync();
        queryable = queryable.Where(t => taskIds.Contains(t.Id));

        // Apply status filter (override the pending-only default if a specific status is requested)
        if (input.Status.HasValue)
        {
            // When filtering by a specific status, query all tasks for this user, not just pending
            queryable = await _taskRepository.GetQueryableAsync();
            queryable = queryable.Where(t => t.AssignedToUserId == userId && t.Status == input.Status.Value);
        }

        // Apply date filters
        if (input.DateFrom.HasValue)
        {
            queryable = queryable.Where(t => t.CreationTime >= input.DateFrom.Value);
        }

        if (input.DateTo.HasValue)
        {
            queryable = queryable.Where(t => t.CreationTime <= input.DateTo.Value);
        }

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable
                .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                .ThenByDescending(t => t.CreationTime);
        }

        // Apply paging
        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var tasks = await AsyncExecuter.ToListAsync(queryable);

        var dtos = tasks.Select(MapToDto).ToList();

        return new PagedResultDto<ApprovalTaskDto>(totalCount, dtos);
    }

    /// <summary>
    /// Approves an approval task assigned to the current user.
    /// Delegates to the domain service which handles delegation verification.
    /// </summary>
    public async Task<ApprovalTaskDto> ApproveAsync(ApproveRejectInput input)
    {
        var userId = CurrentUser.GetId();
        var task = await _workflowManager.ApproveTaskAsync(input.TaskId, userId, input.Comment);
        return MapToDto(task);
    }

    /// <summary>
    /// Rejects an approval task assigned to the current user.
    /// Delegates to the domain service which handles delegation verification.
    /// </summary>
    public async Task<ApprovalTaskDto> RejectAsync(ApproveRejectInput input)
    {
        var userId = CurrentUser.GetId();
        var task = await _workflowManager.RejectTaskAsync(input.TaskId, userId, input.Comment);
        return MapToDto(task);
    }

    #region Private Methods

    private static ApprovalTaskDto MapToDto(ApprovalTask task)
    {
        return new ApprovalTaskDto
        {
            Id = task.Id,
            TenantId = task.TenantId,
            WorkflowInstanceId = task.WorkflowInstanceId,
            TaskName = task.TaskName,
            Description = task.Description,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToRoleName = task.AssignedToRoleName,
            Status = task.Status,
            Comment = task.Comment,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CompletedByUserId = task.CompletedByUserId,
            EntityType = task.EntityType,
            EntityId = task.EntityId,
            CreationTime = task.CreationTime
        };
    }

    #endregion
}
