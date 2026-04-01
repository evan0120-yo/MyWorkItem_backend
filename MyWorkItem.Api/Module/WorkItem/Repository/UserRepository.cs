using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Infrastructure;
using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Repository;

public sealed class UserRepository(MyWorkItemDbContext dbContext) : IUserRepository
{
    public Task<UserEntity?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task AddAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
