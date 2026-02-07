using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Nafath;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace NexusCore.Saudi.Tests.Nafath;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class NafathRepositoryTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IRepository<NafathAuthRequest, Guid> _authRequestRepository;
    private readonly IRepository<NafathUserLink, Guid> _userLinkRepository;

    public NafathRepositoryTests()
    {
        _authRequestRepository = GetRequiredService<IRepository<NafathAuthRequest, Guid>>();
        _userLinkRepository = GetRequiredService<IRepository<NafathUserLink, Guid>>();
    }

    [Fact]
    public async Task Should_Query_AuthRequest_By_TransactionId()
    {
        var request = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _authRequestRepository.GetQueryableAsync();
            return await queryable
                .Where(r => r.TransactionId == NexusCoreTestData.NafathCompletedTransactionId)
                .FirstOrDefaultAsync();
        });

        request.ShouldNotBeNull();
        request.Id.ShouldBe(NexusCoreTestData.NafathCompletedRequestId);
        request.NationalId.ShouldBe(NexusCoreTestData.NafathNationalId);
        request.Status.ShouldBe(NafathRequestStatus.Completed);
    }

    [Fact]
    public async Task Should_Query_AuthRequest_By_NationalId()
    {
        var requests = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _authRequestRepository.GetQueryableAsync();
            return await queryable
                .Where(r => r.NationalId == NexusCoreTestData.NafathNationalId)
                .ToListAsync();
        });

        requests.Count.ShouldBeGreaterThanOrEqualTo(2);
        requests.ShouldContain(r => r.Status == NafathRequestStatus.Completed);
        requests.ShouldContain(r => r.Status == NafathRequestStatus.Expired);
    }

    [Fact]
    public async Task Should_Query_AuthRequest_By_Status()
    {
        var completedRequests = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _authRequestRepository.GetQueryableAsync();
            return await queryable
                .Where(r => r.Status == NafathRequestStatus.Completed)
                .ToListAsync();
        });

        completedRequests.ShouldNotBeEmpty();
        completedRequests.ShouldAllBe(r => r.CompletedAt != null);
    }

    [Fact]
    public async Task Should_Query_UserLink_By_UserId()
    {
        var link = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _userLinkRepository.GetQueryableAsync();
            return await queryable
                .Where(l => l.UserId == NexusCoreTestData.NafathLinkedUserId)
                .FirstOrDefaultAsync();
        });

        link.ShouldNotBeNull();
        link.NationalId.ShouldBe(NexusCoreTestData.NafathNationalId);
        link.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Query_UserLink_By_NationalId()
    {
        var link = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _userLinkRepository.GetQueryableAsync();
            return await queryable
                .Where(l => l.NationalId == NexusCoreTestData.NafathNationalId && l.IsActive)
                .FirstOrDefaultAsync();
        });

        link.ShouldNotBeNull();
        link.UserId.ShouldBe(NexusCoreTestData.NafathLinkedUserId);
    }

    [Fact]
    public async Task Should_Return_Null_For_Unknown_UserId()
    {
        var unknownUserId = Guid.NewGuid();

        var link = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _userLinkRepository.GetQueryableAsync();
            return await queryable
                .Where(l => l.UserId == unknownUserId)
                .FirstOrDefaultAsync();
        });

        link.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Insert_And_Query_New_AuthRequest()
    {
        var newId = Guid.NewGuid();
        var txnId = "NAFATH-TXN-NEW-001";

        await WithUnitOfWorkAsync(async () =>
        {
            var request = new NafathAuthRequest(
                newId, txnId, "9999999999", 55,
                DateTime.UtcNow, DateTime.UtcNow.AddMinutes(2));
            await _authRequestRepository.InsertAsync(request);
        });

        var inserted = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _authRequestRepository.GetQueryableAsync();
            return await queryable
                .Where(r => r.TransactionId == txnId)
                .FirstOrDefaultAsync();
        });

        inserted.ShouldNotBeNull();
        inserted.NationalId.ShouldBe("9999999999");
        inserted.Status.ShouldBe(NafathRequestStatus.Pending);
    }
}
