using Microsoft.EntityFrameworkCore;

namespace MyWorkItem.Api.Infrastructure;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyWorkItemDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }
}
