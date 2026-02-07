using System;
using Volo.Abp.Application.Dtos;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Input DTO for getting a paginated and filtered list of approval tasks
/// </summary>
public class GetApprovalTaskListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by approval status
    /// </summary>
    public ApprovalStatus? Status { get; set; }

    /// <summary>
    /// Filter by creation date from (inclusive)
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter by creation date to (inclusive)
    /// </summary>
    public DateTime? DateTo { get; set; }
}
