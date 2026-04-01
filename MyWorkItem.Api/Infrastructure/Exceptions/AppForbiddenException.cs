namespace MyWorkItem.Api.Infrastructure.Exceptions;

public sealed class AppForbiddenException(string message) : Exception(message);
