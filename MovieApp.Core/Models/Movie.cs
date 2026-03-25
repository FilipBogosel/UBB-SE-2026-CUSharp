#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a movie that can be cataloged, reviewed, and battled.
/// </summary>
public class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }

    public string PosterUrl { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public ICollection<Battle> FirstMovieBattles { get; set; } = new HashSet<Battle>();

    public ICollection<Battle> SecondMovieBattles { get; set; } = new HashSet<Battle>();

    public ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
}
