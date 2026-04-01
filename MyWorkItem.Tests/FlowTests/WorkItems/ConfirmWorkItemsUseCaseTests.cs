using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.WorkItems;

public sealed class ConfirmWorkItemsUseCaseTests
{
    [Fact]
    public async Task ConfirmWorkItems_WhenIdsAreNull_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = null!,
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task ConfirmWorkItems_WhenIdsAreEmpty_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = [],
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task ConfirmWorkItems_WhenIdsContainEmptyGuid_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = [Guid.NewGuid(), Guid.Empty],
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task ConfirmWorkItems_WhenAnyIdDoesNotExist_ThrowsNotFound()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var existingId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = existingId,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

        await dbContext.SaveChangesAsync();

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = [existingId, Guid.NewGuid()],
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppNotFoundException>(action);
        Assert.Empty(dbContext.UserWorkItemStatuses);
        Assert.Empty(dbContext.Users);
    }

    [Fact]
    public async Task ConfirmWorkItems_WhenIdsExist_CreatesConfirmedStatusesForCurrentUser()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId1 = Guid.NewGuid();
        var workItemId2 = Guid.NewGuid();

        dbContext.WorkItems.AddRange(
            new WorkItemEntity
            {
                Id = workItemId1,
                Title = "Work Item 1",
                Description = "Description 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new WorkItemEntity
            {
                Id = workItemId2,
                Title = "Work Item 2",
                Description = "Description 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

        await dbContext.SaveChangesAsync();

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var response = await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = [workItemId1, workItemId2],
            },
            CancellationToken.None);

        Assert.Equal(2, response.ConfirmedCount);
        Assert.All(dbContext.UserWorkItemStatuses, item => Assert.Equal(PersonalStatus.Confirmed, item.Status));
        Assert.Equal(2, dbContext.UserWorkItemStatuses.Count());
        Assert.Single(dbContext.Users);
    }

    [Fact]
    public async Task ConfirmWorkItems_WhenStatusAlreadyExists_DoesNotCreateDuplicateRows()
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

        var useCase = new ConfirmWorkItemsUseCase(
            new ConfirmWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemStatusService(dbContext));

        var response = await useCase.ExecuteAsync(
            new ConfirmWorkItemsRequest
            {
                WorkItemIds = [workItemId, workItemId],
            },
            CancellationToken.None);

        Assert.Equal(1, response.ConfirmedCount);
        Assert.Equal(PersonalStatus.Confirmed, dbContext.UserWorkItemStatuses.Single().Status);
        Assert.Single(dbContext.UserWorkItemStatuses);
    }
}
