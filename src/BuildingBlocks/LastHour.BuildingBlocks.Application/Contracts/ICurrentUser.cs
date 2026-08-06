namespace LastHour.BuildingBlocks.Application.Contracts;

/// <summary>
/// Provides access to the identity of the currently authenticated user, resolved from the
/// ambient request context without coupling the application layer to a web framework.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets a value indicating whether the current request is associated with an
    /// authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the identifier of the current user, or <see langword="null"/> when not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the display name of the current user, or <see langword="null"/> when not authenticated.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the roles the current user belongs to.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Determines whether the current user belongs to the specified role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns><see langword="true"/> when the user is in the role; otherwise <see langword="false"/>.</returns>
    bool IsInRole(string role);
}
