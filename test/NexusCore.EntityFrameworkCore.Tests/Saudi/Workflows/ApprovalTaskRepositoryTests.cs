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
public class ApprovalTaskRepositoryTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly IRepository<ApprovalTask, Guid> _taskRepository;

    public ApprovalTaskRepositoryTests()
    {
        _taskRepository = GetRequiredService<IRepository<ApprovalTask, Guid>>();
    }

    [Fact]
    public async Task Should_Query_By_Status_And_User()
    {
        var tasks = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _taskRepository.GetQueryableAsync();
            return await queryable
                .Where(t =>
                    t.AssignedToUserId == NexusCoreTestData.ApprovalTaskAssigneeUserId &&
                    t.Status == ApprovalStatus.Pending)
                .ToListAsync();
        });

        tasks.ShouldNotBeEmpty();
        tasks.ShouldContain(t => t.Id == NexusCoreTestData.ApprovalTaskPendingId);
        tasks.ShouldAllBe(t => t.Status == ApprovalStatus.Pending);
    }

    [Fact]
    public async Task Should_Query_By_WorkflowInstanceId()
    {
        var task = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _taskRepository.GetQueryableAsync();
            return await queryable
                .Where(t => t.WorkflowInstanceId == "WF-INSTANCE-001")
                .FirstOrDefaultAsync();
        });

        task.ShouldNotBeNull();
        task.Id.ShouldBe(NexusCoreTestData.ApprovalTaskPendingId);
        task.TaskName.ShouldBe("Manager Approval");
    }

    [Fact]
    public async Task Should_Query_By_EntityType_And_EntityId()
    {
        var tasks = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _taskRepository.GetQueryableAsync();
            return await queryable
                .Where(t =>
                    t.EntityType == "ZatcaInvoice" &&
                    t.EntityId == NexusCoreTestData.InvoiceDraftId.ToString())
                .ToListAsync();
        });

        tasks.ShouldNotBeEmpty();
        tasks.ShouldContain(t => t.Id == NexusCoreTestData.ApprovalTaskPendingId);
    }

    [Fact]
    public async Task Should_Query_All_Tasks_For_User()
    {
        var tasks = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _taskRepository.GetQueryableAsync();
            return await queryable
                .Where(t => t.AssignedToUserId == NexusCoreTestData.ApprovalTaskAssigneeUserId)
                .ToListAsync();
        });

        tasks.Count.ShouldBeGreaterThanOrEqualTo(2);
        tasks.ShouldContain(t => t.Status == ApprovalStatus.Pending);
        tasks.ShouldContain(t => t.Status == ApprovalStatus.Approved);
    }

    [Fact]
    public async Task Should_Query_Approved_Tasks()
    {
        var approvedTask = await WithUnitOfWorkAsync(async () =>
            await _taskRepository.GetAsync(NexusCoreTestData.ApprovalTaskApprovedId));

        approvedTask.Status.ShouldBe(ApprovalStatus.Approved);
        approvedTask.CompletedByUserId.ShouldBe(NexusCoreTestData.ApprovalTaskAssigneeUserId);
        approvedTask.CompletedAt.ShouldNotBeNull();
        approvedTask.Comment.ShouldBe("Looks good");
    }

    [Fact]
    public async Task Should_Insert_And_Query_New_Task()
    {
        var newId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var task = new ApprovalTask(
                newId,
                "WF-REPO-TEST-001",
                "Repo Test Task",
                Guid.NewGuid(),
                description: "Test task for repository tests",
                entityType: "TestEntity",
                entityId: "999");
            await _taskRepository.InsertAsync(task);
        });

        var inserted = await WithUnitOfWorkAsync(async () =>
            await _taskRepository.GetAsync(newId));

        inserted.ShouldNotBeNull();
        inserted.TaskName.ShouldBe("Repo Test Task");
        inserted.Status.ShouldBe(ApprovalStatus.Pending);
        inserted.EntityType.ShouldBe("TestEntity");
    }

    [Fact]
    public async Task Should_Support_Sorting_By_DueDate()
    {
        var task1Id = Guid.NewGuid();
        var task2Id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _taskRepository.InsertAsync(new ApprovalTask(
                task1Id, "WF-SORT-001", "Task 1", userId,
                dueDate: DateTime.UtcNow.AddDays(5)));
            await _taskRepository.InsertAsync(new ApprovalTask(
                task2Id, "WF-SORT-002", "Task 2", userId,
                dueDate: DateTime.UtcNow.AddDays(1)));
        });

        var sorted = await WithUnitOfWorkAsync(async () =>
        {
            var queryable = await _taskRepository.GetQueryableAsync();
            return await queryable
                .Where(t => t.AssignedToUserId == userId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        });

        sorted.Count.ShouldBe(2);
        sorted[0].Id.ShouldBe(task2Id);
        sorted[1].Id.ShouldBe(task1Id);
    }
}
