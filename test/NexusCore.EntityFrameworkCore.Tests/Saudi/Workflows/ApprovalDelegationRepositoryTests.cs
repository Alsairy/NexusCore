using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Workflows;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace NexusCore.Saudi.Tests.Workflows;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ApprovalDelegationRepositoryTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IRepository<ApprovalDelegation, Guid> _delegationRepository;

    public ApprovalDelegationRepositoryTests()
    {
        _delegationRepository = GetRequiredService<IRepository<ApprovalDelegation, Guid>>();
    }

    [Fact]
    public async Task Should_Query_Active_Delegations_For_Delegate()
    {
        var delegations = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _delegationRepository.GetQueryableAsync();
            var now = DateTime.UtcNow;
            return await queryable
                .Where(d =>
                    d.DelegateUserId == NexusCoreTestData.DelegateUserId &&
                    d.IsActive &&
                    d.StartDate <= now &&
                    d.EndDate >= now)
                .ToListAsync();
        });

        delegations.ShouldNotBeEmpty();
        delegations.ShouldContain(d => d.Id == NexusCoreTestData.DelegationId);
    }

    [Fact]
    public async Task Should_Query_By_DelegatorUserId()
    {
        var delegations = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _delegationRepository.GetQueryableAsync();
            return await queryable
                .Where(d => d.DelegatorUserId == NexusCoreTestData.DelegatorUserId)
                .ToListAsync();
        });

        delegations.ShouldNotBeEmpty();
        delegations.ShouldContain(d => d.DelegateUserId == NexusCoreTestData.DelegateUserId);
    }

    [Fact]
    public async Task Should_Query_By_DelegateUserId()
    {
        var delegations = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _delegationRepository.GetQueryableAsync();
            return await queryable
                .Where(d => d.DelegateUserId == NexusCoreTestData.DelegateUserId)
                .ToListAsync();
        });

        delegations.ShouldNotBeEmpty();
        delegations.ShouldContain(d => d.DelegatorUserId == NexusCoreTestData.DelegatorUserId);
    }

    [Fact]
    public async Task Should_Return_Seeded_Delegation_Details()
    {
        var delegation = await WithUnitOfWorkAsync(async () =>
            await _delegationRepository.GetAsync(NexusCoreTestData.DelegationId));

        delegation.ShouldNotBeNull();
        delegation.DelegatorUserId.ShouldBe(NexusCoreTestData.DelegatorUserId);
        delegation.DelegateUserId.ShouldBe(NexusCoreTestData.DelegateUserId);
        delegation.IsActive.ShouldBeTrue();
        delegation.Reason.ShouldBe("Annual leave");
        delegation.StartDate.ShouldBeLessThan(DateTime.UtcNow);
        delegation.EndDate.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_Return_Empty_For_Unknown_Delegate()
    {
        var unknownDelegateId = Guid.NewGuid();

        var delegations = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _delegationRepository.GetQueryableAsync();
            return await queryable
                .Where(d => d.DelegateUserId == unknownDelegateId)
                .ToListAsync();
        });

        delegations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Insert_New_Delegation()
    {
        var newId = Guid.NewGuid();
        var delegatorId = Guid.NewGuid();
        var delegateId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var delegation = new ApprovalDelegation(
                newId,
                delegatorId,
                delegateId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(7),
                "Business trip");
            await _delegationRepository.InsertAsync(delegation);
        });

        var inserted = await WithUnitOfWorkAsync(async () =>
            await _delegationRepository.GetAsync(newId));

        inserted.ShouldNotBeNull();
        inserted.DelegatorUserId.ShouldBe(delegatorId);
        inserted.DelegateUserId.ShouldBe(delegateId);
        inserted.Reason.ShouldBe("Business trip");
        inserted.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Filter_Only_Active_Delegations()
    {
        var inactiveId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var delegation = new ApprovalDelegation(
                inactiveId,
                Guid.NewGuid(),
                NexusCoreTestData.DelegateUserId,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-1),
                "Expired delegation");
            delegation.IsActive = false;
            await _delegationRepository.InsertAsync(delegation);
        });

        var activeDelegations = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _delegationRepository.GetQueryableAsync();
            return await queryable
                .Where(d =>
                    d.DelegateUserId == NexusCoreTestData.DelegateUserId &&
                    d.IsActive)
                .ToListAsync();
        });

        activeDelegations.ShouldNotContain(d => d.Id == inactiveId);
        activeDelegations.ShouldContain(d => d.Id == NexusCoreTestData.DelegationId);
    }
}
