using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusCore.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace NexusCore.Saudi.Nafath;

/// <summary>
/// HTTP client for interacting with the Nafath API
/// </summary>
public class NafathApiClient : ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger<NafathApiClient> _logger;

    public NafathApiClient(
        IHttpClientFactory httpClientFactory,
        ISettingProvider settingProvider,
        ILogger<NafathApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    /// <summary>
    /// Initiates authentication with Nafath API
    /// </summary>
    /// <param name="nationalId">The national ID to authenticate</param>
    /// <returns>Transaction ID and random number from Nafath</returns>
    public async Task<NafathInitiateResponse> InitiateAuthAsync(string nationalId)
    {
        var appId = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.AppId);
        var appKey = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.AppKey);
        var apiBaseUrl = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.ApiBaseUrl);

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appKey) || string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new BusinessException("NexusCore:NafathNotConfigured")
                .WithData("Message", "Nafath settings are not properly configured");
        }

        var client = _httpClientFactory.CreateClient("NafathApi");
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("AppId", appId);
        client.DefaultRequestHeaders.Add("AppKey", appKey);

        var request = new
        {
            nationalId = nationalId
        };

        _logger.LogInformation("Initiating Nafath authentication for National ID: {NationalId}", nationalId);

        try
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/initiate", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NafathInitiateResponse>();

            if (result == null)
            {
                throw new BusinessException("NexusCore:NafathApiError")
                    .WithData("Message", "Invalid response from Nafath API");
            }

            _logger.LogInformation("Nafath authentication initiated. Transaction ID: {TransactionId}", result.TransactionId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling Nafath API");
            throw new BusinessException("NexusCore:NafathApiError")
                .WithData("Message", "Failed to communicate with Nafath API");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Nafath API response");
            throw new BusinessException("NexusCore:NafathApiError")
                .WithData("Message", "Invalid response format from Nafath API");
        }
    }

    /// <summary>
    /// Checks the status of an authentication request with Nafath API
    /// </summary>
    /// <param name="transactionId">The transaction ID to check</param>
    /// <returns>Current status from Nafath</returns>
    public async Task<NafathStatusResponse> CheckStatusAsync(string transactionId)
    {
        var appId = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.AppId);
        var appKey = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.AppKey);
        var apiBaseUrl = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Nafath.ApiBaseUrl);

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appKey) || string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new BusinessException("NexusCore:NafathNotConfigured")
                .WithData("Message", "Nafath settings are not properly configured");
        }

        var client = _httpClientFactory.CreateClient("NafathApi");
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("AppId", appId);
        client.DefaultRequestHeaders.Add("AppKey", appKey);

        _logger.LogInformation("Checking Nafath status for Transaction ID: {TransactionId}", transactionId);

        try
        {
            var response = await client.GetAsync($"/api/v1/auth/status/{transactionId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NafathStatusResponse>();

            if (result == null)
            {
                throw new BusinessException("NexusCore:NafathApiError")
                    .WithData("Message", "Invalid response from Nafath API");
            }

            _logger.LogInformation("Nafath status: {Status} for Transaction ID: {TransactionId}", result.Status, transactionId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling Nafath API");
            throw new BusinessException("NexusCore:NafathApiError")
                .WithData("Message", "Failed to communicate with Nafath API");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Nafath API response");
            throw new BusinessException("NexusCore:NafathApiError")
                .WithData("Message", "Invalid response format from Nafath API");
        }
    }
}

/// <summary>
/// Response from Nafath initiate authentication API
/// </summary>
public class NafathInitiateResponse
{
    public string TransactionId { get; set; } = null!;
    public int RandomNumber { get; set; }
}

/// <summary>
/// Response from Nafath status check API
/// </summary>
public class NafathStatusResponse
{
    public string TransactionId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? NationalId { get; set; }
}
