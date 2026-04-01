namespace MyWorkItem.Api.Infrastructure.Exceptions;

public sealed class AppValidationException : Exception
{
    public AppValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
