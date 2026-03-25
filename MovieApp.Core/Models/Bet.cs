#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a user's bet on a battle.
/// </summary>
public class Bet
{
    public int UserId { get; set; }

    public int BattleId { get; set; }

    public int MovieId { get; set; }

    public int Amount { get; set; }

    public User? User { get; set; }

    public Battle? Battle { get; set; }

    public Movie? Movie { get; set; }
}
