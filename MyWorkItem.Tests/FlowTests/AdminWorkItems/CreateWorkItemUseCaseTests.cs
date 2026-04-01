using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.FlowTests.AdminWorkItems;

public sealed class CreateWorkItemUseCaseTests
{
    [Fact]
    public async Task CreateWorkItem_WhenCallerIsAdmin_CreatesWorkItem()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new CreateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new CreateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var response = await useCase.ExecuteAsync(
            new CreateWorkItemRequest
            {
                Title = "Work Item 1",
                Description = "Description 1",
            },
            CancellationToken.None);

        Assert.Equal("Work Item 1", response.Title);
        Assert.Single(dbContext.WorkItems);
        Assert.True(response.UpdatedAt >= response.CreatedAt);
    }

    [Fact]
    public async Task CreateWorkItem_WhenTitleIsEmpty_ThrowsValidation()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new CreateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("admin-1", "admin-1", "Admin One", AppRole.Admin)),
            new CreateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new CreateWorkItemRequest
            {
                Title = "   ",
                Description = "Description 1",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppValidationException>(action);
    }

    [Fact]
    public async Task CreateWorkItem_WhenCallerIsUser_ThrowsForbidden()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();

        var useCase = new CreateWorkItemUseCase(
            TestDependencyFactory.CreateCurrentUserService(
                dbContext,
                new CurrentUser("user-1", "user-1", "User One", AppRole.User)),
            new CreateWorkItemRequestValidator(),
            TestDependencyFactory.CreateWorkItemCommandService(dbContext));

        var action = async () => await useCase.ExecuteAsync(
            new CreateWorkItemRequest
            {
                Title = "Work Item 1",
                Description = "Description 1",
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<AppForbiddenException>(action);
        Assert.Empty(dbContext.WorkItems);
        Assert.Empty(dbContext.Users);
    }
}
