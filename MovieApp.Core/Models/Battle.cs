#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a weekly battle between two movies.
/// </summary>
public class Battle
{
    public int BattleId { get; set; }

    public int FirstMovieId { get; set; }

    public int SecondMovieId { get; set; }

    public double InitialRatingFirstMovie { get; set; }

    public double InitialRatingSecondMovie { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public Movie? FirstMovie { get; set; }

    public Movie? SecondMovie { get; set; }

    public ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
}
