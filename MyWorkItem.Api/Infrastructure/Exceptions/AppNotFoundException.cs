namespace MyWorkItem.Api.Infrastructure.Exceptions;

public sealed class AppNotFoundException(string message) : Exception(message);
