using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Volo.Abp.DependencyInjection;

namespace NexusCore.HealthChecks;

public class NafathApiHealthCheck : IHealthCheck, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public NafathApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["Saudi:Nafath:ApiBaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return HealthCheckResult.Degraded("Nafath API base URL is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("NafathApi");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(baseUrl, cancellationToken);

            return response.IsSuccessStatusCode || (int)response.StatusCode < 500
                ? HealthCheckResult.Healthy($"Nafath API is reachable at {baseUrl}.")
                : HealthCheckResult.Degraded($"Nafath API returned {(int)response.StatusCode} at {baseUrl}.");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("Nafath API request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Degraded($"Nafath API is unreachable: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Nafath API health check failed: {ex.Message}", ex);
        }
    }
}
