using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.ServiceTests;

public sealed class WorkItemCommandServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_SetsCreatedAtAndUpdatedAt()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();
        var service = TestDependencyFactory.CreateWorkItemCommandService(dbContext);

        var response = await service.CreateAsync(
            new CreateWorkItemRequest
            {
                Title = "Work Item 1",
                Description = "Description 1",
            },
            CancellationToken.None);

        Assert.Equal("Work Item 1", response.Title);
        Assert.True(response.UpdatedAt >= response.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_UpdatesAllowedFields()
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

        var service = TestDependencyFactory.CreateWorkItemCommandService(dbContext);

        var response = await service.UpdateAsync(
            workItemId,
            new UpdateWorkItemRequest
            {
                Title = "New Title",
                Description = "New Description",
            },
            CancellationToken.None);

        Assert.Equal("New Title", response.Title);
        Assert.Equal("New Description", dbContext.WorkItems.Single().Description);
        Assert.True(response.UpdatedAt > response.CreatedAt);
    }

    [Fact]
    public async Task DeleteAsync_WhenWorkItemExists_RemovesWorkItem()
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

        var service = TestDependencyFactory.CreateWorkItemCommandService(dbContext);

        await service.DeleteAsync(workItemId, CancellationToken.None);

        Assert.Empty(dbContext.WorkItems);
    }
}
