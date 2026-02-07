using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Input DTO for approving or rejecting an approval task
/// </summary>
public class ApproveRejectInput
{
    /// <summary>
    /// The ID of the task to approve or reject
    /// </summary>
    [Required]
    public Guid TaskId { get; set; }

    /// <summary>
    /// Optional comment explaining the decision
    /// </summary>
    [StringLength(SaudiConsts.MaxDescriptionLength)]
    public string? Comment { get; set; }
}
