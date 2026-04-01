using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MyWorkItem.Api.Infrastructure;

namespace MyWorkItem.Tests.AuthTests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenUnhandledExceptionOccurs_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            logger);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal("Request failed.", root.GetProperty("title").GetString());
        Assert.Equal("An unexpected error occurred.", root.GetProperty("detail").GetString());
    }
}
