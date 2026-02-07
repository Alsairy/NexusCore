using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using NexusCore.Saudi.Nafath.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// Application service for Nafath SSO operations
/// </summary>
public class NafathAppService : NexusCoreAppService, INafathAppService
{
    private readonly NafathAuthManager _authManager;
    private readonly NafathApiClient _apiClient;
    private readonly IRepository<NafathAuthRequest, Guid> _authRequestRepository;
    private readonly ICurrentUser _currentUser;

    public NafathAppService(
        NafathAuthManager authManager,
        NafathApiClient apiClient,
        IRepository<NafathAuthRequest, Guid> authRequestRepository,
        ICurrentUser currentUser)
    {
        _authManager = authManager;
        _apiClient = apiClient;
        _authRequestRepository = authRequestRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Initiates a Nafath login request
    /// </summary>
    [Authorize(NexusCorePermissions.Nafath.Login)]
    public async Task<NafathAuthRequestDto> InitiateLoginAsync(NafathInitiateLoginInput input)
    {
        // Call Nafath API to initiate authentication
        var apiResponse = await _apiClient.InitiateAuthAsync(input.NationalId);

        // Create domain entity with response from API
        var authRequest = await _authManager.CreateAuthRequestAsync(input.NationalId);

        // Update with transaction ID from API if different
        if (authRequest.TransactionId != apiResponse.TransactionId)
        {
            authRequest.TransactionId = apiResponse.TransactionId;
            authRequest.RandomNumber = apiResponse.RandomNumber;
            authRequest.Status = NafathRequestStatus.Waiting;

            await _authRequestRepository.UpdateAsync(authRequest);
        }
        else
        {
            // Update status to waiting
            authRequest.Status = NafathRequestStatus.Waiting;
            await _authRequestRepository.UpdateAsync(authRequest);
        }

        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(authRequest);
    }

    /// <summary>
    /// Checks the status of a Nafath authentication request
    /// </summary>
    [Authorize(NexusCorePermissions.Nafath.Login)]
    public async Task<NafathAuthRequestDto> CheckStatusAsync(NafathCheckStatusInput input)
    {
        // Get the auth request from database
        var authRequest = await _authRequestRepository.FirstOrDefaultAsync(
            x => x.TransactionId == input.TransactionId);

        if (authRequest == null)
        {
            throw new BusinessException("NexusCore:NafathAuthRequestNotFound")
                .WithData("TransactionId", input.TransactionId);
        }

        // If already completed, return current status
        if (authRequest.Status == NafathRequestStatus.Completed ||
            authRequest.Status == NafathRequestStatus.Rejected ||
            authRequest.Status == NafathRequestStatus.Expired ||
            authRequest.Status == NafathRequestStatus.Failed)
        {
            return MapToDto(authRequest);
        }

        // Check if expired
        if (authRequest.ExpiresAt < Clock.Now)
        {
            authRequest.MarkExpired();
            await _authRequestRepository.UpdateAsync(authRequest);
            await CurrentUnitOfWork!.SaveChangesAsync();

            return MapToDto(authRequest);
        }

        // Poll Nafath API for status
        var apiResponse = await _apiClient.CheckStatusAsync(input.TransactionId);

        // Update status based on API response
        var status = apiResponse.Status.ToUpperInvariant();

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

            case "WAITING":
            case "PENDING":
                authRequest.Status = NafathRequestStatus.Waiting;
                break;

            default:
                authRequest.MarkFailed();
                break;
        }

        await _authRequestRepository.UpdateAsync(authRequest);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(authRequest);
    }

    /// <summary>
    /// Links a national ID to the current user's identity after successful authentication
    /// </summary>
    [Authorize(NexusCorePermissions.Nafath.LinkIdentity)]
    public async Task<NafathUserLinkDto> LinkIdentityAsync(NafathInitiateLoginInput input)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new BusinessException("NexusCore:UserNotAuthenticated")
                .WithData("Message", "User must be authenticated to link identity");
        }

        var userId = _currentUser.GetId();

        // Initiate authentication first
        var authResponse = await InitiateLoginAsync(input);

        // Wait for user to complete authentication in Nafath app
        // In a real scenario, this would be done via polling or callback
        // For now, we just verify there's a completed auth request for this national ID

        var completedAuth = await _authRequestRepository.FirstOrDefaultAsync(
            x => x.NationalId == input.NationalId &&
                 x.Status == NafathRequestStatus.Completed);

        if (completedAuth == null)
        {
            throw new BusinessException("NexusCore:NafathAuthenticationNotCompleted")
                .WithData("Message", "Authentication must be completed before linking identity")
                .WithData("TransactionId", authResponse.TransactionId);
        }

        // Link the user
        var link = await _authManager.LinkUserAsync(userId, input.NationalId);

        // Update the auth request with user ID
        completedAuth.MarkCompleted(userId);
        await _authRequestRepository.UpdateAsync(completedAuth);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToLinkDto(link);
    }

    /// <summary>
    /// Gets the current user's linked national ID
    /// </summary>
    [Authorize]
    public async Task<NafathUserLinkDto?> GetMyLinkAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return null;
        }

        var userId = _currentUser.GetId();
        var link = await _authManager.GetActiveLinkAsync(userId);

        return link != null ? MapToLinkDto(link) : null;
    }

    #region Private Methods

    private static NafathAuthRequestDto MapToDto(NafathAuthRequest entity)
    {
        return new NafathAuthRequestDto
        {
            Id = entity.Id,
            TransactionId = entity.TransactionId,
            NationalId = entity.NationalId,
            RandomNumber = entity.RandomNumber,
            Status = entity.Status,
            RequestedAt = entity.RequestedAt,
            ExpiresAt = entity.ExpiresAt,
            CompletedAt = entity.CompletedAt,
            UserId = entity.UserId
        };
    }

    private static NafathUserLinkDto MapToLinkDto(NafathUserLink entity)
    {
        return new NafathUserLinkDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            NationalId = entity.NationalId,
            VerifiedAt = entity.VerifiedAt,
            IsActive = entity.IsActive
        };
    }

    #endregion
}
