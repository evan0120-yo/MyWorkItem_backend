using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public interface IUserWorkItemStatusRepository
{
    Task<IReadOnlyList<UserWorkItemStatusEntity>> GetByUserAndWorkItemIdsAsync(
        string userId,
        IReadOnlyCollection<Guid> workItemIds,
        bool asTracking,
        CancellationToken cancellationToken);

    Task<UserWorkItemStatusEntity?> GetByUserAndWorkItemIdAsync(
        string userId,
        Guid workItemId,
        bool asTracking,
        CancellationToken cancellationToken);

    Task AddRangeAsync(
        IEnumerable<UserWorkItemStatusEntity> statuses,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
