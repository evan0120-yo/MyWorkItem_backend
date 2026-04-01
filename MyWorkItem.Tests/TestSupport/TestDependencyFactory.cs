using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure;
using MyWorkItem.Api.Module.WorkItem.Repository;
using MyWorkItem.Api.Module.WorkItem.Service;

namespace MyWorkItem.Tests.TestSupport;

internal static class TestDependencyFactory
{
    public static MyWorkItemDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MyWorkItemDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MyWorkItemDbContext(options);
    }

    public static CurrentUserService CreateCurrentUserService(
        MyWorkItemDbContext dbContext,
        CurrentUser? currentUser)
    {
        return new CurrentUserService(new TestCurrentUserAccessor(currentUser));
    }

    public static WorkItemQueryService CreateWorkItemQueryService(MyWorkItemDbContext dbContext)
    {
        return new WorkItemQueryService(
            new WorkItemRepository(dbContext),
            new UserWorkItemStatusRepository(dbContext));
    }

    public static WorkItemStatusService CreateWorkItemStatusService(MyWorkItemDbContext dbContext)
    {
        return new WorkItemStatusService(
            new WorkItemRepository(dbContext),
            new UserWorkItemStatusRepository(dbContext),
            new UserRepository(dbContext));
    }

    public static WorkItemCommandService CreateWorkItemCommandService(MyWorkItemDbContext dbContext)
    {
        return new WorkItemCommandService(new WorkItemRepository(dbContext));
    }
}
