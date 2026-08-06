namespace LastHour.BuildingBlocks.SharedKernel.Domain;

/// <summary>
/// Marks an entity that supports soft deletion instead of physical removal.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp in UTC.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}
