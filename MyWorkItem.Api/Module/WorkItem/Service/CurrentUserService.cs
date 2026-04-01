using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;

namespace MyWorkItem.Api.Module.WorkItem.Service;

public sealed class CurrentUserService(ICurrentUserAccessor currentUserAccessor)
{
    public Task<CurrentUser> GetRequiredCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetCurrentUser()
            ?? throw new AppUnauthorizedException("Current user could not be resolved.");

        return Task.FromResult(currentUser);
    }

    public async Task<CurrentUser> EnsureAdminAsync(CancellationToken cancellationToken)
    {
        var currentUser = await GetRequiredCurrentUserAsync(cancellationToken);

        if (currentUser.Role != AppRole.Admin)
        {
            throw new AppForbiddenException("Current user does not have admin permission.");
        }

        return currentUser;
    }
}
