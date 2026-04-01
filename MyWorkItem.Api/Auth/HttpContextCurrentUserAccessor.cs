using Microsoft.AspNetCore.Http;

namespace MyWorkItem.Api.Auth;

public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser? GetCurrentUser()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return null;
        }

        var userId = httpContext.Request.Headers[MockAuthHeaders.UserId].ToString().Trim();
        var userName = httpContext.Request.Headers[MockAuthHeaders.UserName].ToString().Trim();
        var roleRaw = httpContext.Request.Headers[MockAuthHeaders.Role].ToString().Trim();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleRaw))
        {
            return null;
        }

        if (!Enum.TryParse<AppRole>(roleRaw, ignoreCase: true, out var role))
        {
            return null;
        }

        var resolvedUserName = string.IsNullOrWhiteSpace(userName) ? userId : userName;

        return new CurrentUser(
            userId,
            resolvedUserName,
            resolvedUserName,
            role);
    }
}
