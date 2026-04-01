using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Infrastructure;
using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public sealed class UserWorkItemStatusRepository(MyWorkItemDbContext dbContext) : IUserWorkItemStatusRepository
{
    public async Task<IReadOnlyList<UserWorkItemStatusEntity>> GetByUserAndWorkItemIdsAsync(
        string userId,
        IReadOnlyCollection<Guid> workItemIds,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        if (workItemIds.Count == 0)
        {
            return [];
        }

        var query = asTracking ? dbContext.UserWorkItemStatuses : dbContext.UserWorkItemStatuses.AsNoTracking();

        return await query
            .Where(x => x.UserId == userId && workItemIds.Contains(x.WorkItemId))
            .ToListAsync(cancellationToken);
    }

    public Task<UserWorkItemStatusEntity?> GetByUserAndWorkItemIdAsync(
        string userId,
        Guid workItemId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = asTracking ? dbContext.UserWorkItemStatuses : dbContext.UserWorkItemStatuses.AsNoTracking();

        return query.SingleOrDefaultAsync(
            x => x.UserId == userId && x.WorkItemId == workItemId,
            cancellationToken);
    }

    public Task AddRangeAsync(
        IEnumerable<UserWorkItemStatusEntity> statuses,
        CancellationToken cancellationToken)
    {
        return dbContext.UserWorkItemStatuses.AddRangeAsync(statuses, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
