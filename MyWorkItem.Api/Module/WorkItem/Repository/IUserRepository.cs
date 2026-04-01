using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(string userId, CancellationToken cancellationToken);

    Task AddAsync(UserEntity user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
