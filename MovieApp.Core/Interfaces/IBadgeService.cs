#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes badge operations.
/// </summary>
public interface IBadgeService
{
    /// <summary>
    /// Gets badges awarded to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    Task<List<Badge>> GetUserBadges(int userId);

    /// <summary>
    /// Gets all badge definitions.
    /// </summary>
    Task<List<Badge>> GetAllBadges();

    /// <summary>
    /// Gets detailed badge progress for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    Task<List<BadgeProgress>> GetBadgeProgress(int userId);

    /// <summary>
    /// Checks and awards all eligible badges for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    Task CheckAndAwardBadges(int userId);
}
