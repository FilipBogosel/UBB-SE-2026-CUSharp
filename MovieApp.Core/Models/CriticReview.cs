#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a normalized external critic review.
/// </summary>
public class CriticReview
{
    public string Source { get; set; } = string.Empty;

    public double Score { get; set; }

    public string Headline { get; set; } = string.Empty;

    public string Snippet { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
