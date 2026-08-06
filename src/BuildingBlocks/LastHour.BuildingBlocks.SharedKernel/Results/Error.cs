using System.Diagnostics.CodeAnalysis;

namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an expected, non-exceptional failure with a machine-readable code,
/// a human-readable description and a well-known <see cref="ErrorType"/>.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1716",
    Justification = "'Error' is the canonical name for the functional result pattern.")]
public record Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="type">The category of the error.</param>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    protected Error(ErrorType type, string code, string description)
    {
        Type = type;
        Code = code;
        Description = description;
    }

    /// <summary>
    /// Gets the category of the error.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Gets the stable, machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable description of the error.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>An <see cref="Error"/> of type <see cref="ErrorType.Failure"/>.</returns>
    public static Error Failure(string code, string description) => new Error(ErrorType.Failure, code, description);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>A <see cref="ValidationError"/>.</returns>
    public static Error Validation(string code, string description) => new ValidationError(code, description);

    /// <summary>
    /// Creates a not-found error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>A <see cref="NotFoundError"/>.</returns>
    public static Error NotFound(string code, string description) => new NotFoundError(code, description);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>A <see cref="ConflictError"/>.</returns>
    public static Error Conflict(string code, string description) => new ConflictError(code, description);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>An <see cref="UnauthorizedError"/>.</returns>
    public static Error Unauthorized(string code, string description) => new UnauthorizedError(code, description);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <returns>A <see cref="ForbiddenError"/>.</returns>
    public static Error Forbidden(string code, string description) => new ForbiddenError(code, description);

    /// <inheritdoc/>
    public override string ToString() => $"{Type}: {Code} - {Description}";
}
