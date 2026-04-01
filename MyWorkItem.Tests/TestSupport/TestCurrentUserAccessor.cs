using MyWorkItem.Api.Auth;

namespace MyWorkItem.Tests.TestSupport;

internal sealed class TestCurrentUserAccessor(CurrentUser? currentUser) : ICurrentUserAccessor
{
    public CurrentUser? GetCurrentUser()
    {
        return currentUser;
    }
}
