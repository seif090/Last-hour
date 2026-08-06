namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error caused by an operation that conflicts with the current state.
/// </summary>
public sealed record ConflictError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictError"/> class.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    public ConflictError(string code, string description)
        : base(ErrorType.Conflict, code, description)
    {
    }
}
