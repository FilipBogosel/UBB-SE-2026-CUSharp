#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes battle and betting operations.
/// </summary>
public interface IBattleService
{
    /// <summary>
    /// Gets the active battle.
    /// </summary>
    Task<Battle?> GetActiveBattle();

    /// <summary>
    /// Creates a weekly battle.
    /// </summary>
    /// <param name="firstMovieId">The first movie identifier.</param>
    /// <param name="secondMovieId">The second movie identifier.</param>
    Task<Battle> CreateBattle(int firstMovieId, int secondMovieId);

    /// <summary>
    /// Places a bet on a battle.
    /// </summary>
    Task<Bet> PlaceBet(int userId, int battleId, int movieId, int amount);

    /// <summary>
    /// Gets a specific bet for a battle.
    /// </summary>
    Task<Bet?> GetBet(int userId, int battleId);

    /// <summary>
    /// Determines the winner of a battle.
    /// </summary>
    /// <param name="battleId">The battle identifier.</param>
    Task<int> DetermineWinner(int battleId);

    /// <summary>
    /// Distributes payouts for a completed battle.
    /// </summary>
    /// <param name="battleId">The battle identifier.</param>
    Task DistributePayouts(int battleId);
}
