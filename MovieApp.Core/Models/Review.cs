#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a user's review for a movie.
/// </summary>
public class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int MovieId { get; set; }

    public float StarRating { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsExtraReview { get; set; }

    public string? ExtraReviewContent { get; set; }

    public float? CinematographyRating { get; set; }

    public string? CinematographyText { get; set; }

    public float? ActingRating { get; set; }

    public string? ActingText { get; set; }

    public float? CgiRating { get; set; }

    public string? CgiText { get; set; }

    public float? PlotRating { get; set; }

    public string? PlotText { get; set; }

    public float? SoundRating { get; set; }

    public string? SoundText { get; set; }

    public User? User { get; set; }

    public Movie? Movie { get; set; }
}
