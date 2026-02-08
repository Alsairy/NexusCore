using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NexusCore.Saudi.Nafath;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Nafath;

/// <summary>
/// Tests for the NafathCallbackController to verify callback processing,
/// error handling, and input validation. Uses NSubstitute for mocking
/// the INafathAppService dependency.
/// </summary>
public class NafathCallbackTests
{
    private readonly INafathAppService _nafathAppService;
    private readonly ILogger<NafathCallbackController> _logger;
    private readonly NafathCallbackController _controller;

    public NafathCallbackTests()
    {
        _nafathAppService = Substitute.For<INafathAppService>();
        _logger = Substitute.For<ILogger<NafathCallbackController>>();
        _controller = new NafathCallbackController(_nafathAppService, _logger);
    }

    [Fact]
    public async Task Should_Process_Valid_Callback()
    {
        // Arrange
        var request = new NafathCallbackRequest
        {
            TransactionId = "NAFATH-TXN-001",
            Status = "COMPLETED",
            NationalId = "1234567890",
            Timestamp = DateTime.UtcNow
        };

        _nafathAppService.CheckStatusAsync(Arg.Any<NafathCheckStatusInput>())
            .Returns(new NafathAuthRequestDto
            {
                TransactionId = request.TransactionId,
                Status = NafathRequestStatus.Completed
            });

        // Act
        var result = await _controller.ReceiveCallback(request);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        await _nafathAppService.Received(1)
            .CheckStatusAsync(Arg.Is<NafathCheckStatusInput>(
                i => i.TransactionId == "NAFATH-TXN-001"));
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Missing_TransactionId()
    {
        // Arrange
        var request = new NafathCallbackRequest
        {
            TransactionId = "",
            Status = "COMPLETED"
        };

        // Act
        var result = await _controller.ReceiveCallback(request);

        // Assert
        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.StatusCode.ShouldBe(400);

        await _nafathAppService.DidNotReceiveWithAnyArgs()
            .CheckStatusAsync(default!);
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Missing_Status()
    {
        // Arrange
        var request = new NafathCallbackRequest
        {
            TransactionId = "NAFATH-TXN-001",
            Status = ""
        };

        // Act
        var result = await _controller.ReceiveCallback(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();

        await _nafathAppService.DidNotReceiveWithAnyArgs()
            .CheckStatusAsync(default!);
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Null_Request()
    {
        // Act
        var result = await _controller.ReceiveCallback(null!);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Should_Return_Ok_Even_When_Service_Throws()
    {
        // Arrange — Nafath callbacks should return 200 even on error
        // to prevent Nafath from retrying
        var request = new NafathCallbackRequest
        {
            TransactionId = "NAFATH-TXN-UNKNOWN",
            Status = "COMPLETED"
        };

        _nafathAppService.CheckStatusAsync(Arg.Any<NafathCheckStatusInput>())
            .ThrowsAsync(new Exception("Transaction not found"));

        // Act
        var result = await _controller.ReceiveCallback(request);

        // Assert — 200 OK even on error
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Pass_TransactionId_To_Service()
    {
        // Arrange
        var transactionId = "NAFATH-TXN-VERIFY-001";
        var request = new NafathCallbackRequest
        {
            TransactionId = transactionId,
            Status = "REJECTED"
        };

        _nafathAppService.CheckStatusAsync(Arg.Any<NafathCheckStatusInput>())
            .Returns(new NafathAuthRequestDto
            {
                TransactionId = transactionId,
                Status = NafathRequestStatus.Rejected
            });

        // Act
        await _controller.ReceiveCallback(request);

        // Assert
        await _nafathAppService.Received(1)
            .CheckStatusAsync(Arg.Is<NafathCheckStatusInput>(
                i => i.TransactionId == transactionId));
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Ok()
    {
        // Act
        var result = _controller.Health();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
    }
}
