#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes gamification point operations.
/// </summary>
public interface IPointService
{
    /// <summary>
    /// Gets the stats for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    Task<UserStats> GetUserStats(int userId);

    /// <summary>
    /// Adds points for a review-related action.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="movieId">The related movie identifier.</param>
    /// <param name="isBattleMovie">Whether the movie is in the active battle.</param>
    Task AddPoints(int userId, int movieId, bool isBattleMovie);

    /// <summary>
    /// Deducts points from a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="points">The points to deduct.</param>
    Task DeductPoints(int userId, int points);

    /// <summary>
    /// Freezes points for a bet.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="amount">The amount to freeze.</param>
    Task FreezePoints(int userId, int amount);

    /// <summary>
    /// Refunds previously frozen points.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="amount">The amount to refund.</param>
    Task RefundPoints(int userId, int amount);

    /// <summary>
    /// Recalculates the weekly score for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    Task UpdateWeeklyScore(int userId);
}
