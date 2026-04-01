using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Infrastructure;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public sealed class WorkItemRepository(MyWorkItemDbContext dbContext) : IWorkItemRepository
{
    public async Task<IReadOnlyList<WorkItemEntity>> ListAsync(
        WorkItemSortDirection sortDirection,
        CancellationToken cancellationToken)
    {
        var query = dbContext.WorkItems.AsNoTracking();

        query = sortDirection == WorkItemSortDirection.Asc
            ? query.OrderBy(x => x.CreatedAt)
            : query.OrderByDescending(x => x.CreatedAt);

        return await query.ToListAsync(cancellationToken);
    }

    public Task<WorkItemEntity?> GetByIdAsync(
        Guid workItemId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = asTracking ? dbContext.WorkItems : dbContext.WorkItems.AsNoTracking();
        return query.SingleOrDefaultAsync(x => x.Id == workItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItemEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> workItemIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.WorkItems
            .AsNoTracking()
            .Where(x => workItemIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(WorkItemEntity workItem, CancellationToken cancellationToken)
    {
        return dbContext.WorkItems.AddAsync(workItem, cancellationToken).AsTask();
    }

    public void Remove(WorkItemEntity workItem)
    {
        dbContext.WorkItems.Remove(workItem);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
