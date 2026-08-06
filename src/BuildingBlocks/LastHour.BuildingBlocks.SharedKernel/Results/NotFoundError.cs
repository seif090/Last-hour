namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error caused by a missing resource.
/// </summary>
public sealed record NotFoundError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundError"/> class.
    /// </summary>
    /// <param name="code">A stable, machine-readable error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    public NotFoundError(string code, string description)
        : base(ErrorType.NotFound, code, description)
    {
    }
}
