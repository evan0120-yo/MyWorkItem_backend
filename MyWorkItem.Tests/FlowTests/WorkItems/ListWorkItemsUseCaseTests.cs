using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.WorkItems;

public sealed class ListWorkItemsUseCaseTests
{
    [Fact]
    public async Task ListWorkItems_WhenNoUserStatusExists_ReturnsPendingStatus()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        dbContext.WorkItems.AddRange(
            new WorkItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new WorkItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Work Item 2",
                Description = "Description 2",
                CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            });

        await dbContext.SaveChangesAsync();

        var useCase = new ListWorkItemsUseCase(
            new ListWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var response = await useCase.ExecuteAsync(
            new ListWorkItemsRequest(),
            CancellationToken.None);

        Assert.Equal(2, response.Items.Count);
        Assert.All(response.Items, item => Assert.Equal(PersonalStatus.Pending, item.Status));
        Assert.Equal("Work Item 2", response.Items[0].Title);
    }

    [Fact]
    public async Task ListWorkItems_WhenAscendingSortIsRequested_ReturnsAscendingOrder()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        dbContext.WorkItems.AddRange(
            new WorkItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            },
            new WorkItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Work Item 2",
                Description = "Description 2",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });

        await dbContext.SaveChangesAsync();

        var useCase = new ListWorkItemsUseCase(
            new ListWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var response = await useCase.ExecuteAsync(
            new ListWorkItemsRequest
            {
                SortDirection = "asc",
            },
            CancellationToken.None);

        Assert.Equal("Work Item 2", response.Items[0].Title);
        Assert.Equal("Work Item 1", response.Items[1].Title);
    }

    [Fact]
    public async Task ListWorkItems_WhenCurrentUserCannotBeResolved_ThrowsUnauthorized()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new ListWorkItemsUseCase(
            new ListWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(dbContext, currentUser: null),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var action = async () => await useCase.ExecuteAsync(new ListWorkItemsRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<MyWorkItem.Api.Infrastructure.Exceptions.AppUnauthorizedException>(action);
    }
}
