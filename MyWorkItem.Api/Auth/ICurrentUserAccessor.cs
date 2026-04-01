namespace MyWorkItem.Api.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser? GetCurrentUser();
}
