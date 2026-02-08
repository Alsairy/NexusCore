using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NexusCore.Saudi.Nafath;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Nafath;

/// <summary>
/// Integration tests that run against the Nafath sandbox API.
/// These tests require valid Nafath sandbox credentials in appsettings.test.json
/// or environment variables. They are excluded from CI by default.
///
/// Run: dotnet test --filter "Category=Integration&amp;FullyQualifiedName~Nafath"
/// </summary>
[Trait("Category", "Integration")]
public class NafathSandboxTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private readonly string _apiBaseUrl;
    private readonly string _appId;
    private readonly string _appKey;
    private readonly int _timeoutSeconds;

    public NafathSandboxTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("NAFATH_")
            .Build();

        _apiBaseUrl = _configuration["Nafath:Sandbox:ApiBaseUrl"] ?? "https://nafath.api.elm.sa";
        _appId = _configuration["Nafath:Sandbox:AppId"] ?? "";
        _appKey = _configuration["Nafath:Sandbox:AppKey"] ?? "";
        _timeoutSeconds = int.TryParse(_configuration["Nafath:Sandbox:TimeoutSeconds"], out var t)
            ? t
            : NafathConsts.DefaultTimeoutSeconds;

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private bool HasCredentials => !string.IsNullOrEmpty(_appId)
                                && !string.IsNullOrEmpty(_appKey);

    private void ConfigureHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("APP-ID", _appId);
        _httpClient.DefaultRequestHeaders.Add("APP-KEY", _appKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    [Fact]
    public async Task Should_Initiate_Auth_Request_And_Receive_TransactionId()
    {
        if (!HasCredentials) return;

        ConfigureHeaders();

        var payload = new { nationalId = "1234567890" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/initiate", content);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotBeNullOrWhiteSpace();

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Should receive a transactionId
            root.TryGetProperty("transactionId", out var txnId).ShouldBeTrue();
            txnId.GetString().ShouldNotBeNullOrWhiteSpace();

            // Should receive a random number for verification
            root.TryGetProperty("random", out var random).ShouldBeTrue();
        }
        else
        {
            // Even if sandbox rejects, we should get a structured error
            ((int)response.StatusCode).ShouldBeInRange(400, 499);
        }
    }

    [Fact]
    public async Task Should_Check_Status_Returns_Waiting_Or_Completed()
    {
        if (!HasCredentials) return;

        ConfigureHeaders();

        // First initiate to get a transaction ID
        var initPayload = new { nationalId = "1234567890" };
        var initContent = new StringContent(
            JsonSerializer.Serialize(initPayload),
            Encoding.UTF8,
            "application/json");

        var initResponse = await _httpClient.PostAsync(
            $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/initiate", initContent);

        if (!initResponse.IsSuccessStatusCode) return;

        var initBody = await initResponse.Content.ReadAsStringAsync();
        using var initDoc = JsonDocument.Parse(initBody);
        var transactionId = initDoc.RootElement.GetProperty("transactionId").GetString();

        // Check status
        var statusResponse = await _httpClient.GetAsync(
            $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/status/{transactionId}");

        statusResponse.IsSuccessStatusCode.ShouldBeTrue();

        var statusBody = await statusResponse.Content.ReadAsStringAsync();
        using var statusDoc = JsonDocument.Parse(statusBody);
        var status = statusDoc.RootElement.GetProperty("status").GetString();

        // Valid statuses: WAITING, PENDING, COMPLETED, REJECTED, EXPIRED
        var validStatuses = new[] { "WAITING", "PENDING", "COMPLETED", "REJECTED", "EXPIRED" };
        validStatuses.ShouldContain(status?.ToUpperInvariant());
    }

    [Fact]
    public async Task Should_Handle_Expired_Transaction_Gracefully()
    {
        if (!HasCredentials) return;

        ConfigureHeaders();

        // Use a fake/expired transaction ID
        var response = await _httpClient.GetAsync(
            $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/status/EXPIRED-TXN-{Guid.NewGuid():N}");

        var body = await response.Content.ReadAsStringAsync();

        // Should get an error response (404 or 400) rather than 500
        ((int)response.StatusCode).ShouldBeInRange(400, 499);
        body.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Reject_Invalid_National_Id()
    {
        if (!HasCredentials) return;

        ConfigureHeaders();

        // National ID must be exactly 10 digits
        var payload = new { nationalId = "INVALID" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/initiate", content);

        response.IsSuccessStatusCode.ShouldBeFalse();
        ((int)response.StatusCode).ShouldBeInRange(400, 499);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Timeout_After_Configured_Duration()
    {
        if (!HasCredentials) return;

        // Create a client with very short timeout
        using var shortTimeoutClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(1)
        };
        shortTimeoutClient.DefaultRequestHeaders.Add("APP-ID", _appId);
        shortTimeoutClient.DefaultRequestHeaders.Add("APP-KEY", _appKey);

        var payload = new { nationalId = "1234567890" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        await Should.ThrowAsync<TaskCanceledException>(async () =>
        {
            await shortTimeoutClient.PostAsync(
                $"{_apiBaseUrl.TrimEnd('/')}/api/v1/auth/initiate", content);
        });
    }
}
