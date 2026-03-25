#nullable enable

using System.Text.RegularExpressions;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides mocked external review data that can later be swapped for real API calls.
/// </summary>
public class ExternalReviewService
{
    private static readonly HashSet<string> StopWords =
    [
        "the", "and", "for", "with", "that", "this", "from", "into", "about", "their",
        "have", "has", "its", "but", "you", "your", "his", "her", "was", "were", "are",
        "is", "a", "an", "of", "to", "in", "on", "it", "as", "at", "be", "by", "or"
    ];

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalReviewService"/> class.
    /// </summary>
    /// <param name="httpClient">The shared HTTP client.</param>
    public ExternalReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets mocked external critic reviews for a movie.
    /// </summary>
    /// <param name="movieTitle">The movie title.</param>
    public async Task<List<CriticReview>> GetExternalReviews(string movieTitle)
    {
        _ = _httpClient;
        await Task.Delay(350);

        var title = movieTitle.Trim();
        return
        [
            new CriticReview
            {
                Source = "New York Times",
                Score = 8.9,
                Headline = $"{title} is ambitious and emotionally precise",
                Snippet = $"{title} balances spectacle with introspection, creating a surprisingly intimate blockbuster experience.",
                Url = "https://www.nytimes.com/",
            },
            new CriticReview
            {
                Source = "The Guardian",
                Score = 8.2,
                Headline = $"A sharp, stylish take on {title}",
                Snippet = $"The direction is nimble, the performances lock in quickly, and the tone remains confident even when the plot gets thorny.",
                Url = "https://www.theguardian.com/",
            },
            new CriticReview
            {
                Source = "OMDb",
                Score = 7.8,
                Headline = $"{title} earns strong word of mouth",
                Snippet = $"Audiences and critics alike point to memorable visuals, confident pacing, and a strong final act.",
                Url = "https://www.omdbapi.com/",
            },
        ];
    }

    /// <summary>
    /// Gets mocked aggregate critic and audience scores for a movie.
    /// </summary>
    /// <param name="movieTitle">The movie title.</param>
    public async Task<(double CriticScore, double AudienceScore)> GetAggregateScores(string movieTitle)
    {
        var reviews = await GetExternalReviews(movieTitle);
        var criticScore = Math.Round(reviews.Average(review => review.Score), 1);
        var audienceScore = Math.Round(Math.Max(0, criticScore - 0.8 + (movieTitle.Length % 3)), 1);
        return (criticScore, audienceScore);
    }

    /// <summary>
    /// Analyzes frequently used non-stop-words from critic review snippets.
    /// </summary>
    /// <param name="reviews">The reviews to analyze.</param>
    public List<(string Word, int Count)> AnalyseLexicon(List<CriticReview> reviews)
    {
        return reviews
            .SelectMany(review => Regex.Split(review.Snippet.ToLowerInvariant(), @"\W+"))
            .Where(word => !string.IsNullOrWhiteSpace(word) && !StopWords.Contains(word))
            .GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(10)
            .Select(group => (group.Key, group.Count()))
            .ToList();
    }

    /// <summary>
    /// Determines whether critic and audience scores are polarized.
    /// </summary>
    /// <param name="criticScore">The critic score.</param>
    /// <param name="audienceScore">The audience score.</param>
    /// <param name="threshold">The polarization threshold.</param>
    public bool IsPolarized(double criticScore, double audienceScore, double threshold = 2.0)
    {
        return Math.Abs(criticScore - audienceScore) >= threshold;
    }
}
