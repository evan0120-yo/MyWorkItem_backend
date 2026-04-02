using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.AdminWorkItems;

public sealed class UpdateWorkItemUseCaseTests
{
    [Fact]
    public async Task UpdateWorkItem_WhenWorkItemExists_UpdatesFields()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var workItemId = Guid.NewGuid();

        dbContext.WorkItems.Add(
            new WorkItemEntity
            {
                Id = workItemId,
                Title = "Old Title",
                Description = "Old Description",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });

        await dbContext.SaveChangesAsync();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var response = await useCase.ExecuteAsync(
            workItemId,
            new UpdateWorkItemRequest
            {
                Title = "New Title",
                Description = "New Description",
            },
            CancellationToken.None);

        Assert.Equal("New Title", response.Title);
        Assert.Equal("New Description", dbContext.WorkItems.Single().Description);
        Assert.True(dbContext.WorkItems.Single().UpdatedAt > dbContext.WorkItems.Single().CreatedAt);
    }

    [Fact]
    public async Task UpdateWorkItem_WhenTitleIsEmpty_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            Guid.NewGuid(),
            new UpdateWorkItemRequest
            {
                Title = "  ",
                Description = "Description",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task UpdateWorkItem_WhenTitleExceedsMaxLength_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            Guid.NewGuid(),
            new UpdateWorkItemRequest
            {
                Title = new string('T', 201),
                Description = "Description",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task UpdateWorkItem_WhenDescriptionExceedsMaxLength_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            Guid.NewGuid(),
            new UpdateWorkItemRequest
            {
                Title = "New Title",
                Description = new string('D', 2001),
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task UpdateWorkItem_WhenWorkItemDoesNotExist_ThrowsNotFound()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            Guid.NewGuid(),
            new UpdateWorkItemRequest
            {
                Title = "New Title",
                Description = "Description",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppNotFoundException>(action);
    }

    [Fact]
    public async Task UpdateWorkItem_WhenCallerIsUser_ThrowsForbidden()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new UpdateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            new UpdateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            Guid.NewGuid(),
            new UpdateWorkItemRequest
            {
                Title = "New Title",
                Description = "Description",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppForbiddenException>(action);
    }
}
