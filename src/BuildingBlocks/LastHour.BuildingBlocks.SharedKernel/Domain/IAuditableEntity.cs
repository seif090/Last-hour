namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Marks an entity that tracks creation and modification audit information.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp in UTC.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}
