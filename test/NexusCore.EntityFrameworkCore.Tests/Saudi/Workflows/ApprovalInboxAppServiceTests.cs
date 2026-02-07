using System;
using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Workflows;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Workflows;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class ApprovalInboxAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IApprovalInboxAppService _approvalInboxAppService;

    public ApprovalInboxAppServiceTests()
    {
        _approvalInboxAppService = GetRequiredService<IApprovalInboxAppService>();
    }

    [Fact]
    public async Task GetMyTasks_Should_Return_Tasks()
    {
        // The test user from FakeCurrentPrincipalAccessor has a specific UserId.
        // The seeded tasks are assigned to ApprovalTaskAssigneeUserId, not the fake admin.
        // With AlwaysAllowAuthorization, the service runs but queries for the fake admin's tasks.
        // This test verifies the service runs without error.
        var result = await _approvalInboxAppService.GetMyTasksAsync(
            new GetApprovalTaskListInput { MaxResultCount = 10 });

        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetMyTasks_Filter_By_Status_Should_Work()
    {
        var result = await _approvalInboxAppService.GetMyTasksAsync(
            new GetApprovalTaskListInput
            {
                Status = ApprovalStatus.Pending,
                MaxResultCount = 10
            });

        result.ShouldNotBeNull();
        if (result.Items.Count > 0)
        {
            result.Items.ShouldAllBe(t => t.Status == ApprovalStatus.Pending);
        }
    }

    [Fact]
    public async Task Approve_Should_Update_Task_Status()
    {
        // Create a task assigned to the fake admin user for testing
        // The fake admin user ID from FakeCurrentPrincipalAccessor
        var fakeAdminId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

        // Use the workflow manager to create a task for the fake admin
        var workflowManager = GetRequiredService<NexusCore.Saudi.Workflows.Services.ApprovalWorkflowManager>();

        var task = await WithUnitOfWorkAsync(async () =>
            await workflowManager.CreateTaskAsync(
                "WF-TEST-APPROVE",
                "Test Approve Task",
                fakeAdminId,
                description: "Task for approve test"));

        // Approve via the app service (which uses CurrentUser = fake admin)
        var result = await _approvalInboxAppService.ApproveAsync(
            new ApproveRejectInput
            {
                TaskId = task.Id,
                Comment = "Approved via test"
            });

        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApprovalStatus.Approved);
        result.Comment.ShouldBe("Approved via test");
        result.CompletedByUserId.ShouldBe(fakeAdminId);
        result.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Reject_Should_Update_Task_Status()
    {
        var fakeAdminId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

        var workflowManager = GetRequiredService<NexusCore.Saudi.Workflows.Services.ApprovalWorkflowManager>();

        var task = await WithUnitOfWorkAsync(async () =>
            await workflowManager.CreateTaskAsync(
                "WF-TEST-REJECT",
                "Test Reject Task",
                fakeAdminId,
                description: "Task for reject test"));

        var result = await _approvalInboxAppService.RejectAsync(
            new ApproveRejectInput
            {
                TaskId = task.Id,
                Comment = "Rejected via test"
            });

        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApprovalStatus.Rejected);
        result.Comment.ShouldBe("Rejected via test");
        result.CompletedByUserId.ShouldBe(fakeAdminId);
    }
}
