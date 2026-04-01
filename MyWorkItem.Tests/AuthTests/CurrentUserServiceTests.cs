using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Tests.TestSupport;

namespace MyWorkItem.Tests.AuthTests;

public sealed class CurrentUserServiceTests
{
    [Fact]
    public async Task GetRequiredCurrentUser_WhenCurrentUserCannotBeResolved_ThrowsUnauthorized()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();
        var service = TestDependencyFactory.CreateCurrentUserService(dbContext, currentUser: null);

        var action = async () => await service.GetRequiredCurrentUserAsync(CancellationToken.None);

        await Assert.ThrowsAsync<AppUnauthorizedException>(action);
    }

    [Fact]
    public async Task EnsureAdmin_WhenCurrentUserIsNotAdmin_ThrowsForbiddenWithoutWritingUser()
    {
        await using var dbContext = TestDependencyFactory.CreateDbContext();
        var service = TestDependencyFactory.CreateCurrentUserService(
            dbContext,
            new CurrentUser("user-1", "user-1", "User One", AppRole.User));

        var action = async () => await service.EnsureAdminAsync(CancellationToken.None);

        await Assert.ThrowsAsync<AppForbiddenException>(action);
        Assert.Empty(dbContext.Users);
    }
}
