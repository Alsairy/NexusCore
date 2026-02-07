using System;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Full DTO for the ApprovalTask entity
/// </summary>
public class ApprovalTaskDto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string WorkflowInstanceId { get; set; } = string.Empty;

    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid AssignedToUserId { get; set; }

    public string? AssignedToRoleName { get; set; }

    public ApprovalStatus Status { get; set; }

    public string? Comment { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public DateTime CreationTime { get; set; }
}
