using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Nafath;
using NexusCore.Saudi.Nafath.Services;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace NexusCore.Saudi.Tests.Nafath;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class NafathAuthManagerTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly NafathAuthManager _nafathAuthManager;

    public NafathAuthManagerTests()
    {
        _nafathAuthManager = GetRequiredService<NafathAuthManager>();
    }

    [Fact]
    public async Task CreateAuthRequest_Should_Create_With_Pending_Status()
    {
        var request = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.CreateAuthRequestAsync("1234567890"));

        request.ShouldNotBeNull();
        request.NationalId.ShouldBe("1234567890");
        request.Status.ShouldBe(NafathRequestStatus.Pending);
        request.TransactionId.ShouldNotBeNullOrWhiteSpace();
        request.RandomNumber.ShouldBeGreaterThanOrEqualTo(0);
        request.RandomNumber.ShouldBeLessThanOrEqualTo(NafathConsts.RandomNumberMaxValue);
        request.ExpiresAt.ShouldBeGreaterThan(request.RequestedAt);
    }

    [Fact]
    public async Task CreateAuthRequest_Should_Reject_Invalid_NationalId_Length()
    {
        await Should.ThrowAsync<BusinessException>(async () =>
            await WithUnitOfWorkAsync(async () =>
                await _nafathAuthManager.CreateAuthRequestAsync("12345")));
    }

    [Fact]
    public async Task ProcessCallback_Should_Mark_Completed()
    {
        var request = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.CreateAuthRequestAsync("1234567890"));

        var result = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.ProcessCallbackAsync(request.TransactionId, "COMPLETED"));

        result.Status.ShouldBe(NafathRequestStatus.Completed);
        result.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ProcessCallback_Should_Mark_Rejected()
    {
        var request = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.CreateAuthRequestAsync("1234567890"));

        var result = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.ProcessCallbackAsync(request.TransactionId, "REJECTED"));

        result.Status.ShouldBe(NafathRequestStatus.Rejected);
    }

    [Fact]
    public async Task ProcessCallback_Should_Mark_Expired()
    {
        var request = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.CreateAuthRequestAsync("1234567890"));

        var result = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.ProcessCallbackAsync(request.TransactionId, "EXPIRED"));

        result.Status.ShouldBe(NafathRequestStatus.Expired);
    }

    [Fact]
    public async Task ProcessCallback_Should_Throw_For_Unknown_TransactionId()
    {
        await Should.ThrowAsync<BusinessException>(async () =>
            await WithUnitOfWorkAsync(async () =>
                await _nafathAuthManager.ProcessCallbackAsync("NON-EXISTENT", "COMPLETED")));
    }

    [Fact]
    public async Task LinkUser_Should_Create_Active_Link()
    {
        var userId = Guid.NewGuid();
        var nationalId = "9876543210";

        var link = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.LinkUserAsync(userId, nationalId));

        link.ShouldNotBeNull();
        link.UserId.ShouldBe(userId);
        link.NationalId.ShouldBe(nationalId);
        link.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task LinkUser_Should_Reject_NationalId_Already_Linked_To_Another_User()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var nationalId = "5555555555";

        await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.LinkUserAsync(user1, nationalId));

        await Should.ThrowAsync<BusinessException>(async () =>
            await WithUnitOfWorkAsync(async () =>
                await _nafathAuthManager.LinkUserAsync(user2, nationalId)));
    }

    [Fact]
    public async Task GetActiveLink_Should_Return_Seeded_Link()
    {
        var link = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.GetActiveLinkAsync(NexusCoreTestData.NafathLinkedUserId));

        link.ShouldNotBeNull();
        link.NationalId.ShouldBe(NexusCoreTestData.NafathNationalId);
        link.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetActiveLink_Should_Return_Null_For_Unknown_User()
    {
        var link = await WithUnitOfWorkAsync(async () =>
            await _nafathAuthManager.GetActiveLinkAsync(Guid.NewGuid()));

        link.ShouldBeNull();
    }
}
