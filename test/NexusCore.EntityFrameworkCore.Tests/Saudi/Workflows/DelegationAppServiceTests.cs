using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Workflows;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace NexusCore.Saudi.Tests.Workflows;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class DelegationAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IDelegationAppService _delegationAppService;

    public DelegationAppServiceTests()
    {
        _delegationAppService = GetRequiredService<IDelegationAppService>();
    }

    [Fact]
    public async Task GetList_Should_Return_Seeded_Delegation()
    {
        var result = await _delegationAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

        result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Items.ShouldContain(d => d.Id == NexusCoreTestData.DelegationId);
    }

    [Fact]
    public async Task Get_Should_Return_Seeded_Delegation_Details()
    {
        var delegation = await _delegationAppService.GetAsync(NexusCoreTestData.DelegationId);

        delegation.ShouldNotBeNull();
        delegation.DelegatorUserId.ShouldBe(NexusCoreTestData.DelegatorUserId);
        delegation.DelegateUserId.ShouldBe(NexusCoreTestData.DelegateUserId);
        delegation.IsActive.ShouldBeTrue();
        delegation.Reason.ShouldBe("Annual leave");
    }

    [Fact]
    public async Task Create_Should_Add_New_Delegation()
    {
        var delegateUserId = Guid.NewGuid();
        var input = new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = delegateUserId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(14),
            Reason = "Business trip"
        };

        var result = await _delegationAppService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.DelegateUserId.ShouldBe(delegateUserId);
        result.Reason.ShouldBe("Business trip");
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_With_End_Before_Start_Should_Throw()
    {
        var input = new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(5),
            Reason = "Invalid dates"
        };

        await Should.ThrowAsync<BusinessException>(
            () => _delegationAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Delete_Should_Remove_Delegation()
    {
        // Create a delegation to delete
        var created = await _delegationAppService.CreateAsync(new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Reason = "To be deleted"
        });

        await _delegationAppService.DeleteAsync(created.Id);

        var list = await _delegationAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
        list.Items.ShouldNotContain(d => d.Id == created.Id);
    }

    [Fact]
    public async Task GetMyActiveDelegation_Should_Return_Active_Delegation()
    {
        // Create a delegation that covers the current time
        var delegateId = Guid.NewGuid();
        await _delegationAppService.CreateAsync(new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = delegateId,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Reason = "Active delegation"
        });

        var active = await _delegationAppService.GetMyActiveDelegationAsync();

        active.ShouldNotBeNull();
        active.DelegateUserId.ShouldBe(delegateId);
        active.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_Should_Deactivate_Existing_Active_Delegations()
    {
        // Create first delegation
        var first = await _delegationAppService.CreateAsync(new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Reason = "First delegation"
        });

        // Create second delegation — should deactivate the first
        var secondDelegateId = Guid.NewGuid();
        var second = await _delegationAppService.CreateAsync(new CreateUpdateApprovalDelegationDto
        {
            DelegateUserId = secondDelegateId,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(15),
            Reason = "Second delegation"
        });

        // The active delegation should be the second one
        var active = await _delegationAppService.GetMyActiveDelegationAsync();
        active.ShouldNotBeNull();
        active.DelegateUserId.ShouldBe(secondDelegateId);

        // The first delegation should be deactivated
        var firstUpdated = await _delegationAppService.GetAsync(first.Id);
        firstUpdated.IsActive.ShouldBeFalse();
    }
}
