using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace NexusCore.Saudi.Nafath.Services;

/// <summary>
/// Domain service for managing Nafath authentication requests and user identity links
/// </summary>
public class NafathAuthManager : DomainService
{
    private readonly IRepository<NafathAuthRequest, Guid> _authRequestRepository;
    private readonly IRepository<NafathUserLink, Guid> _userLinkRepository;
    private readonly ISettingProvider _settingProvider;

    public NafathAuthManager(
        IRepository<NafathAuthRequest, Guid> authRequestRepository,
        IRepository<NafathUserLink, Guid> userLinkRepository,
        ISettingProvider settingProvider)
    {
        _authRequestRepository = authRequestRepository;
        _userLinkRepository = userLinkRepository;
        _settingProvider = settingProvider;
    }

    /// <summary>
    /// Creates a new Nafath authentication request
    /// </summary>
    /// <param name="nationalId">The national ID to authenticate</param>
    /// <returns>The created NafathAuthRequest</returns>
    public async Task<NafathAuthRequest> CreateAuthRequestAsync(string nationalId)
    {
        Check.NotNullOrWhiteSpace(nationalId, nameof(nationalId));

        if (nationalId.Length != NafathConsts.NationalIdLength)
        {
            throw new BusinessException("NexusCore:InvalidNationalIdLength")
                .WithData("NationalId", nationalId);
        }

        // Generate a unique transaction ID
        var transactionId = Guid.NewGuid().ToString();

        // Generate random number (0-99)
        var random = new Random();
        var randomNumber = random.Next(0, NafathConsts.RandomNumberMaxValue + 1);

        // Get timeout from settings
        var timeoutSeconds = await _settingProvider.GetAsync<int>(
            Settings.NexusCoreSettings.Saudi.Nafath.TimeoutSeconds);

        if (timeoutSeconds <= 0)
        {
            timeoutSeconds = NafathConsts.DefaultTimeoutSeconds;
        }

        var now = Clock.Now;
        var expiresAt = now.AddSeconds(timeoutSeconds);

        var authRequest = new NafathAuthRequest(
            GuidGenerator.Create(),
            transactionId,
            nationalId,
            randomNumber,
            now,
            expiresAt,
            CurrentTenant.Id);

        await _authRequestRepository.InsertAsync(authRequest);

        return authRequest;
    }

    /// <summary>
    /// Processes a callback from Nafath API
    /// </summary>
    /// <param name="transactionId">The transaction ID from the callback</param>
    /// <param name="status">The status from the callback (COMPLETED, REJECTED, EXPIRED)</param>
    /// <returns>The updated NafathAuthRequest</returns>
    public async Task<NafathAuthRequest> ProcessCallbackAsync(string transactionId, string status)
    {
        Check.NotNullOrWhiteSpace(transactionId, nameof(transactionId));
        Check.NotNullOrWhiteSpace(status, nameof(status));

        var authRequest = await _authRequestRepository.FirstOrDefaultAsync(
            x => x.TransactionId == transactionId);

        if (authRequest == null)
        {
            throw new BusinessException("NexusCore:NafathAuthRequestNotFound")
                .WithData("TransactionId", transactionId);
        }

        // Update status based on callback
        status = status.ToUpperInvariant();

        switch (status)
        {
            case "COMPLETED":
                authRequest.MarkCompleted();
                break;

            case "REJECTED":
                authRequest.MarkRejected();
                break;

            case "EXPIRED":
                authRequest.MarkExpired();
                break;

            default:
                authRequest.MarkFailed();
                break;
        }

        await _authRequestRepository.UpdateAsync(authRequest);

        return authRequest;
    }

    /// <summary>
    /// Links a user to a national ID after successful authentication
    /// </summary>
    /// <param name="userId">The user ID to link</param>
    /// <param name="nationalId">The national ID to link</param>
    /// <returns>The created NafathUserLink</returns>
    public async Task<NafathUserLink> LinkUserAsync(Guid userId, string nationalId)
    {
        Check.NotNullOrWhiteSpace(nationalId, nameof(nationalId));

        if (nationalId.Length != NafathConsts.NationalIdLength)
        {
            throw new BusinessException("NexusCore:InvalidNationalIdLength")
                .WithData("NationalId", nationalId);
        }

        // Check if the national ID is already linked to another user
        var existingLink = await _userLinkRepository.FirstOrDefaultAsync(
            x => x.NationalId == nationalId && x.IsActive);

        if (existingLink != null)
        {
            if (existingLink.UserId != userId)
            {
                throw new BusinessException("NexusCore:NationalIdAlreadyLinked")
                    .WithData("NationalId", nationalId);
            }

            // Already linked to this user
            return existingLink;
        }

        // Deactivate any existing links for this user
        var queryable = await _userLinkRepository.GetQueryableAsync();
        var userLinks = queryable.Where(x => x.UserId == userId && x.IsActive).ToList();

        foreach (var link in userLinks)
        {
            link.IsActive = false;
            await _userLinkRepository.UpdateAsync(link);
        }

        // Create new link
        var userLink = new NafathUserLink(
            GuidGenerator.Create(),
            userId,
            nationalId,
            Clock.Now,
            CurrentTenant.Id);

        await _userLinkRepository.InsertAsync(userLink);

        return userLink;
    }

    /// <summary>
    /// Gets the active national ID link for a user
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>The active NafathUserLink, or null if none exists</returns>
    public async Task<NafathUserLink?> GetActiveLinkAsync(Guid userId)
    {
        return await _userLinkRepository.FirstOrDefaultAsync(
            x => x.UserId == userId && x.IsActive);
    }
}
