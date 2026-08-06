namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error caused by an authenticated caller that lacks the required permission.
/// </summary>
public sealed record ForbiddenError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenError"/> class.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    public ForbiddenError(string code, string description)
        : base(ErrorType.Forbidden, code, description)
    {
    }
}
