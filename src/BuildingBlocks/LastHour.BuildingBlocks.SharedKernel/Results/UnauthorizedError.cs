namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error caused by an unauthenticated caller.
/// </summary>
public sealed record UnauthorizedError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedError"/> class.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    public UnauthorizedError(string code, string description)
        : base(ErrorType.Unauthorized, code, description)
    {
    }
}
