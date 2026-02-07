using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using NexusCore.Controllers;

namespace NexusCore.Saudi.Nafath;

[Route("api/saudi/nafath/callback")]
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("NafathCallback")]
public class NafathCallbackController : NexusCoreController
{
    private readonly INafathAppService _nafathAppService;
    private readonly ILogger<NafathCallbackController> _logger;

    public NafathCallbackController(
        INafathAppService nafathAppService,
        ILogger<NafathCallbackController> logger)
    {
        _nafathAppService = nafathAppService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveCallback([FromBody] NafathCallbackRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TransactionId) || string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Received invalid callback from Nafath");
            return BadRequest(new { message = "TransactionId and Status are required" });
        }

        try
        {
            _logger.LogInformation(
                "Processing Nafath callback. TransactionId: {TransactionId}, Status: {Status}",
                request.TransactionId, request.Status);

            await _nafathAppService.CheckStatusAsync(new NafathCheckStatusInput
            {
                TransactionId = request.TransactionId
            });

            return Ok(new { message = "Callback processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing Nafath callback. TransactionId: {TransactionId}",
                request.TransactionId);

            // Return 200 OK even on error to prevent Nafath from retrying
            return Ok(new { message = "Callback received" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "nafath-callback" });
}

public class NafathCallbackRequest
{
    public string TransactionId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? NationalId { get; set; }
    public DateTime? Timestamp { get; set; }
}
