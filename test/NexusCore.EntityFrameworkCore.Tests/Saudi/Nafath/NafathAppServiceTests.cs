using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Nafath;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Nafath;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class NafathAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly INafathAppService _nafathAppService;

    public NafathAppServiceTests()
    {
        _nafathAppService = GetRequiredService<INafathAppService>();
    }

    [Fact]
    public async Task CheckStatus_With_Known_Completed_Transaction_Should_Return_Completed()
    {
        var result = await _nafathAppService.CheckStatusAsync(
            new NafathCheckStatusInput
            {
                TransactionId = NexusCoreTestData.NafathCompletedTransactionId
            });

        result.ShouldNotBeNull();
        result.TransactionId.ShouldBe(NexusCoreTestData.NafathCompletedTransactionId);
        result.NationalId.ShouldBe(NexusCoreTestData.NafathNationalId);
        result.Status.ShouldBe(NafathRequestStatus.Completed);
        result.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CheckStatus_With_Expired_Transaction_Should_Return_Expired()
    {
        var result = await _nafathAppService.CheckStatusAsync(
            new NafathCheckStatusInput
            {
                TransactionId = NexusCoreTestData.NafathExpiredTransactionId
            });

        result.ShouldNotBeNull();
        result.Status.ShouldBe(NafathRequestStatus.Expired);
    }

    [Fact]
    public async Task CheckStatus_With_Unknown_Transaction_Should_Throw()
    {
        await Should.ThrowAsync<Exception>(
            () => _nafathAppService.CheckStatusAsync(
                new NafathCheckStatusInput { TransactionId = "NON-EXISTENT-TXN" }));
    }
}
