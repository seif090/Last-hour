namespace LastHour.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Specifies the category of an <see cref="Error"/>.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Indicates that no error occurred.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a general, unexpected failure.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// Indicates that one or more inputs or the current state failed validation.
    /// </summary>
    Validation = 2,

    /// <summary>
    /// Indicates that a requested resource could not be found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Indicates that the operation conflicted with the current state of the resource.
    /// </summary>
    Conflict = 4,

    /// <summary>
    /// Indicates that the caller could not be authenticated.
    /// </summary>
    Unauthorized = 5,

    /// <summary>
    /// Indicates that the caller is authenticated but lacks permission to perform the operation.
    /// </summary>
    Forbidden = 6,
}
