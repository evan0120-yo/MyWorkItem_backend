using Microsoft.AspNetCore.Http;
using MyWorkItem.Api.Auth;

namespace MyWorkItem.Tests.AuthTests;

public sealed class HttpContextCurrentUserAccessorTests
{
    [Fact]
    public void GetCurrentUser_WhenHeadersAreValid_ReturnsCurrentUser()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[MockAuthHeaders.UserId] = "user-1";
        httpContext.Request.Headers[MockAuthHeaders.UserName] = "user-name";
        httpContext.Request.Headers[MockAuthHeaders.Role] = "Admin";

        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext,
        });

        var currentUser = accessor.GetCurrentUser();

        Assert.NotNull(currentUser);
        Assert.Equal("user-1", currentUser!.UserId);
        Assert.Equal("user-name", currentUser.UserName);
        Assert.Equal(AppRole.Admin, currentUser.Role);
    }

    [Fact]
    public void GetCurrentUser_WhenRequiredHeadersAreMissing_ReturnsNull()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[MockAuthHeaders.UserId] = "user-1";

        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext,
        });

        var currentUser = accessor.GetCurrentUser();

        Assert.Null(currentUser);
    }
}
