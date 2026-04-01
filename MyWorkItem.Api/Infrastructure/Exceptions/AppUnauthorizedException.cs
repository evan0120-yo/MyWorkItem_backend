namespace MyWorkItem.Api.Infrastructure.Exceptions;

public sealed class AppUnauthorizedException(string message) : Exception(message);
