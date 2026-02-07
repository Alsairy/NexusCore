using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// Application service interface for Nafath SSO operations
/// </summary>
public interface INafathAppService : IApplicationService
{
    /// <summary>
    /// Initiates a Nafath login request
    /// </summary>
    /// <param name="input">The national ID to authenticate</param>
    /// <returns>Authentication request details including transaction ID and random number</returns>
    Task<NafathAuthRequestDto> InitiateLoginAsync(NafathInitiateLoginInput input);

    /// <summary>
    /// Checks the status of a Nafath authentication request
    /// </summary>
    /// <param name="input">The transaction ID to check</param>
    /// <returns>Updated authentication request status</returns>
    Task<NafathAuthRequestDto> CheckStatusAsync(NafathCheckStatusInput input);

    /// <summary>
    /// Links a national ID to the current user's identity after successful authentication
    /// </summary>
    /// <param name="input">The national ID to link</param>
    /// <returns>The created identity link</returns>
    Task<NafathUserLinkDto> LinkIdentityAsync(NafathInitiateLoginInput input);

    /// <summary>
    /// Gets the current user's linked national ID
    /// </summary>
    /// <returns>The active identity link, or null if none exists</returns>
    Task<NafathUserLinkDto?> GetMyLinkAsync();
}
