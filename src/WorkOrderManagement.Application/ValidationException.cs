namespace WorkOrderManagement.Application;

/// <summary>
/// Thrown when domain/application-level validation fails.
/// Contains one or more human-readable error messages.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base(string.Join(" ", errors))
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : base(error)
    {
        Errors = [error];
    }
}
