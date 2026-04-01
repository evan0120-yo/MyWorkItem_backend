using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.ServiceTests;

public sealed class WorkItemStatusServiceTests
{
    [Fact]
    public async Task ConfirmAsync_WhenStatusAlreadyExists_UpdatesInsteadOfCreatingDuplicate()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = workItemId,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

        dbContext.Users.Add(
            new UserEntity
            {
                Id = "user-1",
                UserName = "user-1",
                DisplayName = "User One",
            });

        dbContext.UserWorkItemStatuses.Add(
            new UserWorkItemStatusEntity
            {
                UserId = "user-1",
                WorkItemId = workItemId,
                Status = PersonalStatus.Pending,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
            });

        await dbContext.SaveChangesAsync();

        var service = TestDependencyFactory.CreateWorkItemStatusService(dbContext);

        await service.ConfirmAsync(
            [workItemId],
            new CurrentUser("user-1", "user-1", "User One", AppRole.User),
            CancellationToken.None);

        Assert.Single(dbContext.UserWorkItemStatuses);
        Assert.Equal(PersonalStatus.Confirmed, dbContext.UserWorkItemStatuses.Single().Status);
    }

    [Fact]
    public async Task RevertAsync_WhenOtherUserHasStatus_OnlyCurrentUserBecomesPending()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = workItemId,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

        dbContext.Users.AddRange(
            new UserEntity
            {
                Id = "user-1",
                UserName = "user-1",
                DisplayName = "User One",
            },
            new UserEntity
            {
                Id = "user-2",
                UserName = "user-2",
                DisplayName = "User Two",
            });

        dbContext.UserWorkItemStatuses.AddRange(
            new UserWorkItemStatusEntity
            {
                UserId = "user-1",
                WorkItemId = workItemId,
                Status = PersonalStatus.Confirmed,
                UpdatedAt = DateTime.UtcNow,
            },
            new UserWorkItemStatusEntity
            {
                UserId = "user-2",
                WorkItemId = workItemId,
                Status = PersonalStatus.Confirmed,
                UpdatedAt = DateTime.UtcNow,
            });

        await dbContext.SaveChangesAsync();

        var service = TestDependencyFactory.CreateWorkItemStatusService(dbContext);

        var response = await service.RevertAsync(workItemId, "user-1", CancellationToken.None);

        Assert.Equal(PersonalStatus.Pending, response.Status);
        Assert.Equal(PersonalStatus.Pending, dbContext.UserWorkItemStatuses.Single(x => x.UserId == "user-1").Status);
        Assert.Equal(PersonalStatus.Confirmed, dbContext.UserWorkItemStatuses.Single(x => x.UserId == "user-2").Status);
    }
}
