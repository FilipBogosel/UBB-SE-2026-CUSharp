#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides review operations.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly MovieAppDbContext _dbContext;
    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="pointService">The point service.</param>
    /// <param name="badgeService">The badge service.</param>
    public ReviewService(MovieAppDbContext dbContext, IPointService pointService, IBadgeService badgeService)
    {
        _dbContext = dbContext;
        _pointService = pointService;
        _badgeService = badgeService;
    }

    /// <inheritdoc />
    public async Task<List<Review>> GetReviewsForMovie(int movieId)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.MovieId == movieId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Review> AddReview(int userId, int movieId, float rating, string? content)
    {
        ValidateRating(rating);
        ValidateMainReviewContent(content);

        if (await _dbContext.Reviews.AnyAsync(review => review.UserId == userId && review.MovieId == movieId))
        {
            throw new InvalidOperationException("A user can only review a movie once.");
        }

        var movie = await _dbContext.Movies.FirstOrDefaultAsync(item => item.MovieId == movieId)
            ?? throw new InvalidOperationException("Movie not found.");

        var review = new Review
        {
            UserId = userId,
            MovieId = movieId,
            StarRating = rating,
            Content = content?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsExtraReview = false,
        };

        await _dbContext.Reviews.AddAsync(review);
        await _dbContext.SaveChangesAsync();

        movie.AverageRating = await RecalculateAverageRating(movieId);
        await _dbContext.SaveChangesAsync();

        var isBattleMovie = await _dbContext.Battles.AnyAsync(battle =>
            battle.Status == "Active" &&
            (battle.FirstMovieId == movieId || battle.SecondMovieId == movieId));

        await _pointService.AddPoints(userId, movieId, isBattleMovie);
        return review;
    }

    /// <inheritdoc />
    public async Task UpdateReview(int reviewId, float rating, string? content)
    {
        ValidateRating(rating);
        ValidateMainReviewContent(content);

        var review = await _dbContext.Reviews.FirstOrDefaultAsync(item => item.ReviewId == reviewId)
            ?? throw new InvalidOperationException("Review not found.");

        review.StarRating = rating;
        review.Content = content?.Trim() ?? string.Empty;
        await _dbContext.SaveChangesAsync();

        var movie = await _dbContext.Movies.FirstAsync(item => item.MovieId == review.MovieId);
        movie.AverageRating = await RecalculateAverageRating(review.MovieId);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteReview(int reviewId)
    {
        var review = await _dbContext.Reviews.FirstOrDefaultAsync(item => item.ReviewId == reviewId)
            ?? throw new InvalidOperationException("Review not found.");

        var movieId = review.MovieId;
        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        var movie = await _dbContext.Movies.FirstAsync(item => item.MovieId == movieId);
        movie.AverageRating = await RecalculateAverageRating(movieId);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SubmitExtraReview(
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
        string mainExtraText)
    {
        ValidateExtraReview(rating: cgRating, text: cgText, nameof(cgText));
        ValidateExtraReview(rating: actingRating, text: actingText, nameof(actingText));
        ValidateExtraReview(rating: plotRating, text: plotText, nameof(plotText));
        ValidateExtraReview(rating: soundRating, text: soundText, nameof(soundText));
        ValidateExtraReview(rating: cinRating, text: cinText, nameof(cinText));

        if (string.IsNullOrWhiteSpace(mainExtraText) || mainExtraText.Trim().Length is < 500 or > 12000)
        {
            throw new ArgumentException("The extra review text must be between 500 and 12000 characters.", nameof(mainExtraText));
        }

        var review = await _dbContext.Reviews.FirstOrDefaultAsync(item => item.ReviewId == reviewId)
            ?? throw new InvalidOperationException("Review not found.");

        review.IsExtraReview = true;
        review.ExtraReviewContent = mainExtraText.Trim();
        review.CgiRating = cgRating;
        review.CgiText = cgText.Trim();
        review.ActingRating = actingRating;
        review.ActingText = actingText.Trim();
        review.PlotRating = plotRating;
        review.PlotText = plotText.Trim();
        review.SoundRating = soundRating;
        review.SoundText = soundText.Trim();
        review.CinematographyRating = cinRating;
        review.CinematographyText = cinText.Trim();

        await _dbContext.SaveChangesAsync();
        await _badgeService.CheckAndAwardBadges(review.UserId);
    }

    /// <inheritdoc />
    public async Task<double> GetAverageRating(int movieId)
    {
        return await RecalculateAverageRating(movieId);
    }

    private static void ValidateRating(float rating)
    {
        var scaled = rating * 2;
        if (rating < 0 || rating > 5 || Math.Abs(scaled - MathF.Round(scaled)) > 0.001f)
        {
            throw new ArgumentException("Ratings must be between 0 and 5 in 0.5 increments.", nameof(rating));
        }
    }

    private static void ValidateMainReviewContent(string? content)
    {
        if (content is not null && content.Length > 5000)
        {
            throw new ArgumentException("Review content must be 5000 characters or fewer.", nameof(content));
        }
    }

    private static void ValidateExtraReview(float rating, string text, string parameterName)
    {
        ValidateRating(rating);
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length is < 50 or > 2000)
        {
            throw new ArgumentException("Category review text must be between 50 and 2000 characters.", parameterName);
        }
    }

    private async Task<double> RecalculateAverageRating(int movieId)
    {
        return await _dbContext.Reviews
            .Where(review => review.MovieId == movieId)
            .Select(review => (double?)review.StarRating)
            .AverageAsync() ?? 0;
    }
}
