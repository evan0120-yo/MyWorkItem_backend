using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.WorkItems;

public sealed class RevertWorkItemConfirmationUseCaseTests
{
    [Fact]
    public async Task RevertWorkItemConfirmation_WhenOtherUserHasStatus_DoesNotChangeOtherUser()
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

        var useCase = new RevertWorkItemConfirmationUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var response = await useCase.ExecuteAsync(workItemId, CancellationToken.None);

        Assert.Equal(PersonalStatus.Pending, response.Status);
        Assert.Equal(PersonalStatus.Pending, dbContext.UserWorkItemStatuses.Single(x => x.UserId == "user-1").Status);
        Assert.Equal(PersonalStatus.Confirmed, dbContext.UserWorkItemStatuses.Single(x => x.UserId == "user-2").Status);
    }

    [Fact]
    public async Task RevertWorkItemConfirmation_WhenWorkItemDoesNotExist_ThrowsNotFound()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new RevertWorkItemConfirmationUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var action = async () => await useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<AppNotFoundException>(action);
    }
}
