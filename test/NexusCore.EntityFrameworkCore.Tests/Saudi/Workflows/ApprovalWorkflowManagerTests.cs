using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Workflows;
using NexusCore.Saudi.Workflows.Services;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace NexusCore.Saudi.Tests.Workflows;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ApprovalWorkflowManagerTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly ApprovalWorkflowManager _workflowManager;

    public ApprovalWorkflowManagerTests()
    {
        _workflowManager = GetRequiredService<ApprovalWorkflowManager>();
    }

    [Fact]
    public async Task CreateTask_Should_Create_Pending_Task()
    {
        var userId = Guid.NewGuid();

        var task = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.CreateTaskAsync(
                "WF-TEST-001",
                "Test Approval",
                userId,
                description: "Test task",
                entityType: "TestEntity",
                entityId: "123"));

        task.ShouldNotBeNull();
        task.WorkflowInstanceId.ShouldBe("WF-TEST-001");
        task.TaskName.ShouldBe("Test Approval");
        task.AssignedToUserId.ShouldBe(userId);
        task.Status.ShouldBe(ApprovalStatus.Pending);
    }

    [Fact]
    public async Task ApproveTask_By_Assignee_Should_Succeed()
    {
        var result = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.ApproveTaskAsync(
                NexusCoreTestData.ApprovalTaskPendingId,
                NexusCoreTestData.ApprovalTaskAssigneeUserId,
                "Approved by assignee"));

        result.Status.ShouldBe(ApprovalStatus.Approved);
        result.CompletedByUserId.ShouldBe(NexusCoreTestData.ApprovalTaskAssigneeUserId);
        result.Comment.ShouldBe("Approved by assignee");
        result.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ApproveTask_By_Delegate_Should_Succeed()
    {
        var task = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.CreateTaskAsync(
                "WF-DELEGATE-001",
                "Delegation Test",
                NexusCoreTestData.DelegatorUserId));

        var result = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.ApproveTaskAsync(
                task.Id,
                NexusCoreTestData.DelegateUserId,
                "Approved by delegate"));

        result.Status.ShouldBe(ApprovalStatus.Approved);
        result.CompletedByUserId.ShouldBe(NexusCoreTestData.DelegateUserId);
    }

    [Fact]
    public async Task RejectTask_By_Unauthorized_User_Should_Throw()
    {
        var unauthorizedUser = Guid.NewGuid();

        await Should.ThrowAsync<BusinessException>(async () =>
            await WithUnitOfWorkAsync(async () =>
                await _workflowManager.RejectTaskAsync(
                    NexusCoreTestData.ApprovalTaskPendingId,
                    unauthorizedUser,
                    "Should not work")));
    }

    [Fact]
    public async Task GetPendingTasks_Should_Include_Direct_Tasks()
    {
        var tasks = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.GetPendingTasksForUserAsync(
                NexusCoreTestData.ApprovalTaskAssigneeUserId));

        tasks.ShouldNotBeEmpty();
        tasks.ShouldContain(t => t.Id == NexusCoreTestData.ApprovalTaskPendingId);
    }

    [Fact]
    public async Task GetPendingTasks_Should_Include_Delegated_Tasks()
    {
        var task = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.CreateTaskAsync(
                "WF-DELEGATED-LIST",
                "Delegated Task",
                NexusCoreTestData.DelegatorUserId));

        var tasks = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.GetPendingTasksForUserAsync(
                NexusCoreTestData.DelegateUserId));

        tasks.ShouldContain(t => t.Id == task.Id);
    }

    [Fact]
    public async Task CheckDelegation_Should_Return_Delegator()
    {
        var delegatorId = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.CheckDelegationAsync(
                NexusCoreTestData.DelegateUserId));

        delegatorId.ShouldBe(NexusCoreTestData.DelegatorUserId);
    }

    [Fact]
    public async Task CheckDelegation_Should_Return_Null_For_No_Delegation()
    {
        var delegatorId = await WithUnitOfWorkAsync(async () =>
            await _workflowManager.CheckDelegationAsync(Guid.NewGuid()));

        delegatorId.ShouldBeNull();
    }
}
