using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NexusCore.Saudi.Zatca;
using NSubstitute;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

/// <summary>
/// Tests for the ZatcaWebhookController verifying HMAC-SHA256 signature
/// validation, error handling, and security behavior.
/// </summary>
public class ZatcaWebhookSecurityTests
{
    private const string TestWebhookSecret = "test-webhook-secret-for-hmac-validation";

    private readonly ILogger<ZatcaWebhookController> _logger;

    public ZatcaWebhookSecurityTests()
    {
        _logger = Substitute.For<ILogger<ZatcaWebhookController>>();
    }

    #region Helper Methods

    private static string ComputeHmacSha256(string data, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hash);
    }

    private ZatcaWebhookController CreateController(
        IConfiguration configuration,
        string? bodyJson = null,
        string? signatureHeader = null)
    {
        var controller = new ZatcaWebhookController(_logger, configuration);

        // Set up HttpContext with Request
        var httpContext = new DefaultHttpContext();

        if (bodyJson != null)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
            httpContext.Request.Body = new MemoryStream(bodyBytes);
            httpContext.Request.ContentLength = bodyBytes.Length;
            httpContext.Request.ContentType = "application/json";
        }

        if (signatureHeader != null)
        {
            httpContext.Request.Headers["X-ZATCA-Signature"] = signatureHeader;
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static IConfiguration CreateConfig(string? webhookSecret = null)
    {
        var configData = new System.Collections.Generic.Dictionary<string, string?>();

        if (webhookSecret != null)
        {
            configData["Saudi:Zatca:WebhookSecret"] = webhookSecret;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #endregion

    #region HMAC Computation Tests

    [Fact]
    public void HmacSha256_Should_Produce_Deterministic_Output()
    {
        var hash1 = ComputeHmacSha256("test data", "secret");
        var hash2 = ComputeHmacSha256("test data", "secret");

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HmacSha256_Should_Differ_For_Different_Data()
    {
        var hash1 = ComputeHmacSha256("data A", "secret");
        var hash2 = ComputeHmacSha256("data B", "secret");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void HmacSha256_Should_Differ_For_Different_Secrets()
    {
        var hash1 = ComputeHmacSha256("same data", "secret1");
        var hash2 = ComputeHmacSha256("same data", "secret2");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void HmacSha256_Should_Produce_Lowercase_Hex()
    {
        var hash = ComputeHmacSha256("test", "key");

        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldBe(hash.ToLowerInvariant());
        hash.Length.ShouldBe(64); // SHA-256 = 32 bytes = 64 hex chars
    }

    #endregion

    #region Webhook Signature Validation Tests

    [Fact]
    public async Task Should_Accept_Valid_Hmac_Signature()
    {
        // Arrange
        var config = CreateConfig(TestWebhookSecret);
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "test-invoice-hash",
            RequestId = "REQ-001",
            Status = "CLEARED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        var validSignature = ComputeHmacSha256(bodyJson, TestWebhookSecret);

        var controller = CreateController(config, bodyJson, validSignature);

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Reject_Invalid_Hmac_Signature()
    {
        // Arrange
        var config = CreateConfig(TestWebhookSecret);
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "test-invoice-hash",
            Status = "CLEARED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        var controller = CreateController(config, bodyJson, "invalid-signature-value");

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert
        var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
        unauthorizedResult.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task Should_Reject_Missing_Signature_Header()
    {
        // Arrange
        var config = CreateConfig(TestWebhookSecret);
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "test-invoice-hash",
            Status = "CLEARED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        // No signature header
        var controller = CreateController(config, bodyJson, signatureHeader: null);

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Should_Accept_All_When_No_Secret_Configured()
    {
        // Arrange — no webhook secret means dev/sandbox mode
        var config = CreateConfig(webhookSecret: null);
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "test-invoice-hash",
            Status = "REPORTED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        // No signature header and no secret → should accept
        var controller = CreateController(config, bodyJson, signatureHeader: null);

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert — accepted because no secret is configured
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Accept_When_Secret_Is_Empty_String()
    {
        // Arrange — empty string secret also means dev mode
        var config = CreateConfig(webhookSecret: "");
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "test-invoice-hash",
            Status = "REPORTED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        var controller = CreateController(config, bodyJson, signatureHeader: null);

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    #endregion

    #region Request Validation Tests

    [Fact]
    public async Task Should_Return_BadRequest_For_Missing_InvoiceHash()
    {
        // Arrange — no secret configured so signature check passes
        var config = CreateConfig(webhookSecret: null);
        var request = new ZatcaWebhookRequest
        {
            InvoiceHash = "",
            Status = "CLEARED"
        };

        var bodyJson = JsonSerializer.Serialize(request);
        var controller = CreateController(config, bodyJson);

        // Act
        var result = await controller.ReceiveWebhook(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Null_Request()
    {
        // Arrange
        var config = CreateConfig(webhookSecret: null);
        var controller = CreateController(config, "null");

        // Act
        var result = await controller.ReceiveWebhook(null!);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Ok()
    {
        // Arrange
        var config = CreateConfig();
        var controller = new ZatcaWebhookController(_logger, config);

        // Act
        var result = controller.Health();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
    }

    #endregion

    #region Signature Tamper Tests

    [Fact]
    public async Task Should_Reject_When_Body_Is_Tampered_After_Signing()
    {
        // Arrange — sign with original body, then send tampered body
        var config = CreateConfig(TestWebhookSecret);
        var originalBody = JsonSerializer.Serialize(new ZatcaWebhookRequest
        {
            InvoiceHash = "original-hash",
            Status = "CLEARED"
        });

        var signature = ComputeHmacSha256(originalBody, TestWebhookSecret);

        // Tampered body
        var tamperedBody = JsonSerializer.Serialize(new ZatcaWebhookRequest
        {
            InvoiceHash = "tampered-hash",
            Status = "CLEARED"
        });

        var controller = CreateController(config, tamperedBody, signature);

        var tamperedRequest = new ZatcaWebhookRequest
        {
            InvoiceHash = "tampered-hash",
            Status = "CLEARED"
        };

        // Act
        var result = await controller.ReceiveWebhook(tamperedRequest);

        // Assert — tampered body should be rejected
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}
