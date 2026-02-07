using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Full DTO for the ApprovalDelegation entity
/// </summary>
public class ApprovalDelegationDto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid DelegatorUserId { get; set; }

    public Guid DelegateUserId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Reason { get; set; }

    public DateTime CreationTime { get; set; }
}

/// <summary>
/// DTO for creating or updating an approval delegation
/// </summary>
public class CreateUpdateApprovalDelegationDto
{
    /// <summary>
    /// The user who will act as delegate
    /// </summary>
    [Required]
    public Guid DelegateUserId { get; set; }

    /// <summary>
    /// When the delegation becomes active
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// When the delegation expires
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional reason for the delegation
    /// </summary>
    [StringLength(SaudiConsts.MaxDescriptionLength)]
    public string? Reason { get; set; }
}
