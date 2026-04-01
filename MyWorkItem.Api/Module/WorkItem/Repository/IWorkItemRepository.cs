using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public interface IWorkItemRepository
{
    Task<IReadOnlyList<WorkItemEntity>> ListAsync(
        WorkItemSortDirection sortDirection,
        CancellationToken cancellationToken);

    Task<WorkItemEntity?> GetByIdAsync(
        Guid workItemId,
        bool asTracking,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkItemEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> workItemIds,
        CancellationToken cancellationToken);

    Task AddAsync(WorkItemEntity workItem, CancellationToken cancellationToken);

    void Remove(WorkItemEntity workItem);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
