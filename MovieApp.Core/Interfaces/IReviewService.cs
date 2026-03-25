#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes review operations.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Gets all reviews for a movie.
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    Task<List<Review>> GetReviewsForMovie(int movieId);

    /// <summary>
    /// Adds a review for a movie.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="movieId">The movie identifier.</param>
    /// <param name="rating">The star rating.</param>
    /// <param name="content">The review content.</param>
    Task<Review> AddReview(int userId, int movieId, float rating, string? content);

    /// <summary>
    /// Updates an existing review.
    /// </summary>
    /// <param name="reviewId">The review identifier.</param>
    /// <param name="rating">The new star rating.</param>
    /// <param name="content">The new review content.</param>
    Task UpdateReview(int reviewId, float rating, string? content);

    /// <summary>
    /// Deletes a review.
    /// </summary>
    /// <param name="reviewId">The review identifier.</param>
    Task DeleteReview(int reviewId);

    /// <summary>
    /// Submits extra review details.
    /// </summary>
    Task SubmitExtraReview(
        int reviewId,
        float cgRating,
        string cgText,
        float actingRating,
        string actingText,
        float plotRating,
        string plotText,
        float soundRating,
        string soundText,
        float cinRating,
        string cinText,
        string mainExtraText);

    /// <summary>
    /// Gets the current movie average rating.
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    Task<double> GetAverageRating(int movieId);
}
