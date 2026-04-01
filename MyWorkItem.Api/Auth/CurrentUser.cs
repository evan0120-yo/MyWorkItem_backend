namespace MyWorkItem.Api.Auth;

public sealed record CurrentUser(
    string UserId,
    string UserName,
    string DisplayName,
    AppRole Role);
