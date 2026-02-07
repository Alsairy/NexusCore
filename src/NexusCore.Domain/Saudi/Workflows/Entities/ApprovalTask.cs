using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Represents an approval task assigned to a user within a workflow instance.
/// Tracks the lifecycle of a single approval step from creation through completion.
/// </summary>
public class ApprovalTask : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The workflow instance this task belongs to (correlates with Elsa when integrated)
    /// </summary>
    public string WorkflowInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of this approval step (e.g., "Manager Approval", "Finance Review")
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what needs to be approved
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The user assigned to approve/reject this task
    /// </summary>
    public Guid AssignedToUserId { get; set; }

    /// <summary>
    /// Optional role name for role-based task assignment
    /// </summary>
    public string? AssignedToRoleName { get; set; }

    /// <summary>
    /// Current status of the approval task
    /// </summary>
    public ApprovalStatus Status { get; set; }

    /// <summary>
    /// Comment provided by the approver when completing the task
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Optional due date for this approval task
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// When the task was completed (approved, rejected, escalated, or delegated)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// The user who actually completed the task (may differ from AssignedToUserId if delegated)
    /// </summary>
    public Guid? CompletedByUserId { get; set; }

    /// <summary>
    /// The type of entity being approved (e.g., "ZatcaInvoice", "PurchaseOrder")
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// The ID of the entity being approved
    /// </summary>
    public string? EntityId { get; set; }

    protected ApprovalTask() { }

    public ApprovalTask(
        Guid id,
        string workflowInstanceId,
        string taskName,
        Guid assignedToUserId,
        string? description = null,
        string? assignedToRoleName = null,
        DateTime? dueDate = null,
        string? entityType = null,
        string? entityId = null,
        Guid? tenantId = null)
        : base(id)
    {
        WorkflowInstanceId = Check.NotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
        TaskName = Check.NotNullOrWhiteSpace(taskName, nameof(taskName));
        AssignedToUserId = assignedToUserId;
        Description = description;
        AssignedToRoleName = assignedToRoleName;
        DueDate = dueDate;
        EntityType = entityType;
        EntityId = entityId;
        TenantId = tenantId;
        Status = ApprovalStatus.Pending;
    }

    /// <summary>
    /// Approves the task with an optional comment
    /// </summary>
    public void Approve(Guid userId, string? comment = null)
    {
        EnsurePending();
        Status = ApprovalStatus.Approved;
        Comment = comment;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }

    /// <summary>
    /// Rejects the task with an optional comment
    /// </summary>
    public void Reject(Guid userId, string? comment = null)
    {
        EnsurePending();
        Status = ApprovalStatus.Rejected;
        Comment = comment;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }

    /// <summary>
    /// Escalates the task to a higher authority
    /// </summary>
    public void Escalate()
    {
        EnsurePending();
        Status = ApprovalStatus.Escalated;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Delegates the task to another user
    /// </summary>
    public void Delegate(Guid delegateUserId)
    {
        EnsurePending();
        Status = ApprovalStatus.Delegated;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = delegateUserId;
    }

    private void EnsurePending()
    {
        if (Status != ApprovalStatus.Pending)
        {
            throw new BusinessException("NexusCore:ApprovalTaskNotPending")
                .WithData("TaskId", Id)
                .WithData("CurrentStatus", Status.ToString());
        }
    }
}
