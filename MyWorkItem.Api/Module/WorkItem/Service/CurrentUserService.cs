using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;

namespace MyWorkItem.Api.Module.WorkItem.Service;

public sealed class CurrentUserService(ICurrentUserAccessor currentUserAccessor)
{
    public Task<CurrentUser> GetRequiredCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetCurrentUser()
            ?? throw new AppUnauthorizedException("無法解析目前使用者。");

        return Task.FromResult(currentUser);
    }

    public async Task<CurrentUser> EnsureAdminAsync(CancellationToken cancellationToken)
    {
        var currentUser = await GetRequiredCurrentUserAsync(cancellationToken);

        if (currentUser.Role != AppRole.Admin)
        {
            throw new AppForbiddenException("目前使用者沒有管理員權限。");
        }

        return currentUser;
    }
}
