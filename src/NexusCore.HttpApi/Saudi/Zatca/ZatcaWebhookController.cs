using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusCore.Controllers;

namespace NexusCore.Saudi.Zatca;

[Route("api/saudi/zatca/webhook")]
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("ZatcaWebhook")]
public class ZatcaWebhookController : NexusCoreController
{
    private readonly ILogger<ZatcaWebhookController> _logger;
    private readonly IConfiguration _configuration;

    public ZatcaWebhookController(
        ILogger<ZatcaWebhookController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] ZatcaWebhookRequest request)
    {
        // HMAC signature validation
        if (!await ValidateSignatureAsync())
        {
            _logger.LogWarning("ZATCA webhook signature validation failed");
            return Unauthorized(new { message = "Invalid signature" });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.InvoiceHash))
        {
            _logger.LogWarning("Received invalid webhook from ZATCA");
            return BadRequest(new { message = "InvoiceHash is required" });
        }

        try
        {
            _logger.LogInformation(
                "Processing ZATCA webhook. InvoiceHash: {InvoiceHash}, Status: {Status}, RequestId: {RequestId}",
                request.InvoiceHash, request.Status, request.RequestId);

            // TODO: Process the asynchronous ZATCA response
            // Update invoice status based on clearance/reporting result
            await Task.CompletedTask;

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ZATCA webhook. InvoiceHash: {InvoiceHash}",
                request.InvoiceHash);

            // Return 200 to prevent ZATCA from retrying
            return Ok(new { message = "Webhook received" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "zatca-webhook" });

    private async Task<bool> ValidateSignatureAsync()
    {
        var webhookSecret = _configuration["Saudi:Zatca:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            // If no secret configured, skip validation (dev/sandbox mode)
            return true;
        }

        if (!Request.Headers.TryGetValue("X-ZATCA-Signature", out var signatureHeader))
        {
            return false;
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var expectedSignature = ComputeHmacSha256(body, webhookSecret);
        return string.Equals(signatureHeader.ToString(), expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hash);
    }
}

public class ZatcaWebhookRequest
{
    public string InvoiceHash { get; set; } = null!;
    public string? RequestId { get; set; }
    public string? Status { get; set; }
    public string? ClearedInvoice { get; set; }
    public string? Warnings { get; set; }
    public string? Errors { get; set; }
    public DateTime? Timestamp { get; set; }
}
