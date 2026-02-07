using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace NexusCore.Saudi.Workflows.Services;

/// <summary>
/// Domain service for managing approval workflow tasks and delegation logic.
/// Provides the core business logic for the lightweight approval workflow infrastructure.
/// Designed to integrate with Elsa workflow engine when .NET 10 compatibility is available.
/// </summary>
public class ApprovalWorkflowManager : DomainService
{
    private readonly IRepository<ApprovalTask, Guid> _taskRepository;
    private readonly IRepository<ApprovalDelegation, Guid> _delegationRepository;

    public ApprovalWorkflowManager(
        IRepository<ApprovalTask, Guid> taskRepository,
        IRepository<ApprovalDelegation, Guid> delegationRepository)
    {
        _taskRepository = taskRepository;
        _delegationRepository = delegationRepository;
    }

    /// <summary>
    /// Creates a new approval task within a workflow instance
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance this task belongs to</param>
    /// <param name="taskName">Human-readable name for this approval step</param>
    /// <param name="assignedToUserId">The user responsible for this approval</param>
    /// <param name="description">Optional description of what needs to be approved</param>
    /// <param name="assignedToRoleName">Optional role name for role-based assignment</param>
    /// <param name="dueDate">Optional deadline for this approval</param>
    /// <param name="entityType">The type of entity being approved (e.g., "ZatcaInvoice")</param>
    /// <param name="entityId">The ID of the entity being approved</param>
    /// <returns>The created ApprovalTask</returns>
    public async Task<ApprovalTask> CreateTaskAsync(
        string workflowInstanceId,
        string taskName,
        Guid assignedToUserId,
        string? description = null,
        string? assignedToRoleName = null,
        DateTime? dueDate = null,
        string? entityType = null,
        string? entityId = null)
    {
        Check.NotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
        Check.NotNullOrWhiteSpace(taskName, nameof(taskName));

        var task = new ApprovalTask(
            GuidGenerator.Create(),
            workflowInstanceId,
            taskName,
            assignedToUserId,
            description,
            assignedToRoleName,
            dueDate,
            entityType,
            entityId,
            CurrentTenant.Id);

        await _taskRepository.InsertAsync(task);

        return task;
    }

    /// <summary>
    /// Approves a task. Checks if the acting user has an active delegation from the assigned user.
    /// If the userId is neither the assigned user nor a valid delegate, a BusinessException is thrown.
    /// </summary>
    /// <param name="taskId">The ID of the task to approve</param>
    /// <param name="userId">The user performing the approval</param>
    /// <param name="comment">Optional comment explaining the approval</param>
    /// <returns>The updated ApprovalTask</returns>
    public async Task<ApprovalTask> ApproveTaskAsync(Guid taskId, Guid userId, string? comment = null)
    {
        var task = await _taskRepository.GetAsync(taskId);

        await EnsureUserCanActOnTaskAsync(task, userId);

        task.Approve(userId, comment);
        await _taskRepository.UpdateAsync(task);

        return task;
    }

    /// <summary>
    /// Rejects a task. Checks if the acting user has an active delegation from the assigned user.
    /// If the userId is neither the assigned user nor a valid delegate, a BusinessException is thrown.
    /// </summary>
    /// <param name="taskId">The ID of the task to reject</param>
    /// <param name="userId">The user performing the rejection</param>
    /// <param name="comment">Optional comment explaining the rejection</param>
    /// <returns>The updated ApprovalTask</returns>
    public async Task<ApprovalTask> RejectTaskAsync(Guid taskId, Guid userId, string? comment = null)
    {
        var task = await _taskRepository.GetAsync(taskId);

        await EnsureUserCanActOnTaskAsync(task, userId);

        task.Reject(userId, comment);
        await _taskRepository.UpdateAsync(task);

        return task;
    }

    /// <summary>
    /// Gets all pending tasks assigned to the specified user, including tasks
    /// delegated to them by other users with active delegations.
    /// </summary>
    /// <param name="userId">The user to retrieve pending tasks for</param>
    /// <returns>List of pending approval tasks</returns>
    public async Task<List<ApprovalTask>> GetPendingTasksForUserAsync(Guid userId)
    {
        // Get users who have delegated to this user
        var delegationQueryable = await _delegationRepository.GetQueryableAsync();
        var now = Clock.Now;

        var delegatorUserIds = delegationQueryable
            .Where(d => d.DelegateUserId == userId
                        && d.IsActive
                        && d.StartDate <= now
                        && d.EndDate >= now)
            .Select(d => d.DelegatorUserId)
            .ToList();

        // Get pending tasks assigned directly to this user OR to users who delegated to them
        var taskQueryable = await _taskRepository.GetQueryableAsync();
        var allTargetUserIds = new List<Guid> { userId };
        allTargetUserIds.AddRange(delegatorUserIds);

        var pendingTasks = taskQueryable
            .Where(t => t.Status == ApprovalStatus.Pending
                        && allTargetUserIds.Contains(t.AssignedToUserId))
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenBy(t => t.CreationTime);

        return await AsyncExecuter.ToListAsync(pendingTasks);
    }

    /// <summary>
    /// Checks if the specified user has an active delegation from another user.
    /// Returns the delegator's user ID if an active delegation exists.
    /// </summary>
    /// <param name="userId">The user to check delegations for</param>
    /// <returns>The delegator's user ID if an active delegation exists, null otherwise</returns>
    public async Task<Guid?> CheckDelegationAsync(Guid userId)
    {
        var now = Clock.Now;

        var delegation = await _delegationRepository.FirstOrDefaultAsync(
            d => d.DelegateUserId == userId
                 && d.IsActive
                 && d.StartDate <= now
                 && d.EndDate >= now);

        return delegation?.DelegatorUserId;
    }

    #region Private Methods

    /// <summary>
    /// Ensures that the given user is authorized to act on the task.
    /// The user must either be the assigned user, or have an active delegation from the assigned user.
    /// </summary>
    private async Task EnsureUserCanActOnTaskAsync(ApprovalTask task, Guid userId)
    {
        if (task.AssignedToUserId == userId)
        {
            return; // Direct assignee can always act
        }

        // Check if the user is a delegate of the assigned user
        var now = Clock.Now;
        var hasDelegation = await _delegationRepository.AnyAsync(
            d => d.DelegatorUserId == task.AssignedToUserId
                 && d.DelegateUserId == userId
                 && d.IsActive
                 && d.StartDate <= now
                 && d.EndDate >= now);

        if (!hasDelegation)
        {
            throw new BusinessException("NexusCore:NotAuthorizedToActOnTask")
                .WithData("TaskId", task.Id)
                .WithData("UserId", userId);
        }
    }

    #endregion
}
