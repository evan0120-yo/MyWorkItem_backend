using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.WorkItems;

public sealed class GetWorkItemDetailUseCaseTests
{
    [Fact]
    public async Task GetWorkItemDetail_WhenWorkItemDoesNotExist_ThrowsNotFound()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new GetWorkItemDetailUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var action = async () => await useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<AppNotFoundException>(action);
    }

    [Fact]
    public async Task GetWorkItemDetail_WhenUserStatusExists_ReturnsPersonalStatus()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = workItemId,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
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
                Status = PersonalStatus.Confirmed,
                UpdatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            });

        await dbContext.SaveChangesAsync();

        var useCase = new GetWorkItemDetailUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var response = await useCase.ExecuteAsync(workItemId, CancellationToken.None);

        Assert.Equal(workItemId, response.Id);
        Assert.Equal("Work Item 1", response.Title);
        Assert.Equal("Description 1", response.Description);
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.CreatedAt);
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.UpdatedAt);
        Assert.Equal(PersonalStatus.Confirmed, response.Status);
    }

    [Fact]
    public async Task GetWorkItemDetail_WhenUserStatusDoesNotExist_ReturnsPendingStatus()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = workItemId,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            });

        await dbContext.SaveChangesAsync();

        var useCase = new GetWorkItemDetailUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var response = await useCase.ExecuteAsync(workItemId, CancellationToken.None);

        Assert.Equal(workItemId, response.Id);
        Assert.Equal("Work Item 1", response.Title);
        Assert.Equal("Description 1", response.Description);
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.CreatedAt);
        Assert.Equal(new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc), response.UpdatedAt);
        Assert.Equal(PersonalStatus.Pending, response.Status);
    }
}
