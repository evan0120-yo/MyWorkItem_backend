using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.AdminWorkItems;

public sealed class DeleteWorkItemUseCaseTests
{
    [Fact]
    public async Task DeleteWorkItem_WhenWorkItemExists_RemovesWorkItem()
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

        await dbContext.SaveChangesAsync();

        var useCase = new DeleteWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        await useCase.ExecuteAsync(workItemId, CancellationToken.None);

        Assert.Empty(dbContext.WorkItems);
    }

    [Fact]
    public async Task DeleteWorkItem_WhenDeleteSucceeds_ListAndDetailCanNoLongerFindWorkItem()
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

        await dbContext.SaveChangesAsync();

        var deleteUseCase = new DeleteWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        await deleteUseCase.ExecuteAsync(workItemId, CancellationToken.None);

        var listUseCase = new ListWorkItemsUseCase(
            new ListWorkItemsRequestValidator(),
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var detailUseCase = new GetWorkItemDetailUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemQueryService(dbContext));

        var listResponse = await listUseCase.ExecuteAsync(new ListWorkItemsRequest(), CancellationToken.None);
        var detailAction = async () => await detailUseCase.ExecuteAsync(workItemId, CancellationToken.None);

        Assert.Empty(listResponse.Items);
        await Assert.ThrowsAsync<AppNotFoundException>(detailAction);
    }

    [Fact]
    public async Task DeleteWorkItem_WhenWorkItemDoesNotExist_ThrowsNotFound()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new DeleteWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<AppNotFoundException>(action);
    }

    [Fact]
    public async Task DeleteWorkItem_WhenCallerIsUser_ThrowsForbidden()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new DeleteWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<AppForbiddenException>(action);
    }
}
