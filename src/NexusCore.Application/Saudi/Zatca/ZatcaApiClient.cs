using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusCore.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace NexusCore.Saudi.Zatca;

public class ZatcaApiClient : ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger<ZatcaApiClient> _logger;

    public ZatcaApiClient(
        IHttpClientFactory httpClientFactory,
        ISettingProvider settingProvider,
        ILogger<ZatcaApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    public async Task<ZatcaSubmitResultDto> ComplianceCheckAsync(string invoiceXml, string invoiceId)
    {
        _logger.LogInformation("Starting compliance check for invoice {InvoiceId}", invoiceId);

        var apiBaseUrl = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ApiBaseUrl);
        var csid = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ComplianceCsid);
        var secret = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.Secret);

        ValidateSettings(apiBaseUrl, csid, secret);

        var endpoint = $"{apiBaseUrl!.TrimEnd('/')}/compliance/invoices";
        return await SubmitInvoiceAsync(endpoint, invoiceXml, invoiceId, csid!, secret!);
    }

    public async Task<ZatcaSubmitResultDto> ReportInvoiceAsync(string invoiceXml, string invoiceId)
    {
        _logger.LogInformation("Reporting simplified invoice {InvoiceId}", invoiceId);

        var apiBaseUrl = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ApiBaseUrl);
        var csid = await GetProductionCsidAsync();
        var secret = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.Secret);

        ValidateSettings(apiBaseUrl, csid, secret);

        var endpoint = $"{apiBaseUrl!.TrimEnd('/')}/invoices/reporting/single";
        return await SubmitInvoiceAsync(endpoint, invoiceXml, invoiceId, csid!, secret!);
    }

    public async Task<ZatcaSubmitResultDto> ClearInvoiceAsync(string invoiceXml, string invoiceId)
    {
        _logger.LogInformation("Clearing standard invoice {InvoiceId}", invoiceId);

        var apiBaseUrl = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ApiBaseUrl);
        var csid = await GetProductionCsidAsync();
        var secret = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.Secret);

        ValidateSettings(apiBaseUrl, csid, secret);

        var endpoint = $"{apiBaseUrl!.TrimEnd('/')}/invoices/clearance/single";
        return await SubmitInvoiceAsync(endpoint, invoiceXml, invoiceId, csid!, secret!);
    }

    private static void ValidateSettings(string? apiBaseUrl, string? csid, string? secret)
    {
        if (string.IsNullOrEmpty(apiBaseUrl) || string.IsNullOrEmpty(csid) || string.IsNullOrEmpty(secret))
        {
            throw new UserFriendlyException("ZATCA API settings are not configured properly");
        }
    }

    private async Task<string> GetProductionCsidAsync()
    {
        var environment = await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.Environment);
        var csid = environment == "Production"
            ? await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ProductionCsid)
            : await _settingProvider.GetOrNullAsync(NexusCoreSettings.Saudi.Zatca.ComplianceCsid);

        return csid ?? throw new UserFriendlyException("ZATCA CSID is not configured");
    }

    private async Task<ZatcaSubmitResultDto> SubmitInvoiceAsync(
        string endpoint, string invoiceXml, string invoiceId, string csid, string secret)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ZatcaApi");

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{csid}:{secret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                invoiceHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(invoiceXml)),
                uuid = invoiceId,
                invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(invoiceXml))
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("ZATCA API Response: {ResponseContent}", responseContent);

            return response.IsSuccessStatusCode
                ? ParseSuccessResponse(responseContent)
                : ParseErrorResponse(responseContent, (int)response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for invoice {InvoiceId}", invoiceId);
            return new ZatcaSubmitResultDto
            {
                Status = ZatcaInvoiceStatus.Rejected,
                Errors = [$"HTTP request failed: {ex.Message}"]
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for invoice {InvoiceId}", invoiceId);
            return new ZatcaSubmitResultDto
            {
                Status = ZatcaInvoiceStatus.Rejected,
                Errors = [$"Unexpected error: {ex.Message}"]
            };
        }
    }

    private ZatcaSubmitResultDto ParseSuccessResponse(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var validationResults = root.GetProperty("validationResults");
            var status = validationResults.GetProperty("status").GetString();

            var warnings = new List<string>();
            var errors = new List<string>();

            if (validationResults.TryGetProperty("warningMessages", out var warningMessages))
            {
                warnings = warningMessages.EnumerateArray()
                    .Select(w => w.GetProperty("message").GetString() ?? string.Empty)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            }

            if (validationResults.TryGetProperty("errorMessages", out var errorMessages))
            {
                errors = errorMessages.EnumerateArray()
                    .Select(e => e.GetProperty("message").GetString() ?? string.Empty)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            }

            var qrCode = root.TryGetProperty("qrCode", out var qr) ? qr.GetString() : null;
            var requestId = root.TryGetProperty("requestId", out var reqId) ? reqId.GetString() ?? "" : "";

            var invoiceStatus = status?.ToUpperInvariant() switch
            {
                "PASS" => warnings.Count > 0 ? ZatcaInvoiceStatus.Reported : ZatcaInvoiceStatus.Cleared,
                "FAIL" => ZatcaInvoiceStatus.Rejected,
                _ => ZatcaInvoiceStatus.Rejected
            };

            return new ZatcaSubmitResultDto
            {
                Status = invoiceStatus,
                Warnings = warnings,
                Errors = errors,
                QrCode = qrCode,
                RequestId = requestId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ZATCA success response");
            return new ZatcaSubmitResultDto
            {
                Status = ZatcaInvoiceStatus.Rejected,
                Errors = [$"Failed to parse response: {ex.Message}"]
            };
        }
    }

    private static ZatcaSubmitResultDto ParseErrorResponse(string responseContent, int statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var errors = new List<string>();

            if (root.TryGetProperty("errors", out var errorArray))
            {
                errors = errorArray.EnumerateArray()
                    .Select(e => e.GetProperty("message").GetString() ?? string.Empty)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            }
            else if (root.TryGetProperty("message", out var message))
            {
                errors.Add(message.GetString() ?? "Unknown error");
            }

            if (errors.Count == 0)
            {
                errors.Add($"HTTP {statusCode}: {responseContent}");
            }

            return new ZatcaSubmitResultDto
            {
                Status = ZatcaInvoiceStatus.Rejected,
                Errors = errors
            };
        }
        catch
        {
            return new ZatcaSubmitResultDto
            {
                Status = ZatcaInvoiceStatus.Rejected,
                Errors = [$"HTTP {statusCode}: {responseContent}"]
            };
        }
    }
}
