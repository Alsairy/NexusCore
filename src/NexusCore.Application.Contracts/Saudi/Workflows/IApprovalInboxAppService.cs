using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Workflows;

/// <summary>
/// Application service interface for the approval inbox.
/// Provides operations for viewing and acting on approval tasks assigned to the current user.
/// </summary>
public interface IApprovalInboxAppService : IApplicationService
{
    /// <summary>
    /// Gets the paginated list of approval tasks assigned to the current user,
    /// including tasks delegated to the current user by other users.
    /// </summary>
    /// <param name="input">Paging, sorting, and filter parameters</param>
    /// <returns>Paged list of approval task DTOs</returns>
    Task<PagedResultDto<ApprovalTaskDto>> GetMyTasksAsync(GetApprovalTaskListInput input);

    /// <summary>
    /// Approves an approval task assigned to the current user
    /// </summary>
    /// <param name="input">The task ID and optional comment</param>
    /// <returns>The updated approval task DTO</returns>
    Task<ApprovalTaskDto> ApproveAsync(ApproveRejectInput input);

    /// <summary>
    /// Rejects an approval task assigned to the current user
    /// </summary>
    /// <param name="input">The task ID and optional comment</param>
    /// <returns>The updated approval task DTO</returns>
    Task<ApprovalTaskDto> RejectAsync(ApproveRejectInput input);
}
