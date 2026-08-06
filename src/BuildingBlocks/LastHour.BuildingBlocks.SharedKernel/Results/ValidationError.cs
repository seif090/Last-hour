namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error caused by one or more validation failures.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <param name="validations">The individual validation failures that caused this error.</param>
    public ValidationError(string code, string description, IReadOnlyCollection<Error>? validations = null)
        : base(ErrorType.Validation, code, description)
    {
        Validations = validations?.ToArray() ?? Array.Empty<Error>();
    }

    /// <summary>
    /// Gets the individual validation failures that caused this error.
    /// </summary>
    public IReadOnlyList<Error> Validations { get; }
}
