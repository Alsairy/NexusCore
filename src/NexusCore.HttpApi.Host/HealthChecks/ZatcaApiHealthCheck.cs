using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Volo.Abp.DependencyInjection;

namespace NexusCore.HealthChecks;

public class ZatcaApiHealthCheck : IHealthCheck, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ZatcaApiHealthCheck(
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
        var baseUrl = _configuration["Saudi:Zatca:ApiBaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return HealthCheckResult.Degraded("ZATCA API base URL is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ZatcaApi");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(baseUrl, cancellationToken);

            return response.IsSuccessStatusCode || (int)response.StatusCode < 500
                ? HealthCheckResult.Healthy($"ZATCA API is reachable at {baseUrl}.")
                : HealthCheckResult.Degraded($"ZATCA API returned {(int)response.StatusCode} at {baseUrl}.");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("ZATCA API request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Degraded($"ZATCA API is unreachable: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"ZATCA API health check failed: {ex.Message}", ex);
        }
    }
}
