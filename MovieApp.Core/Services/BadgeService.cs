#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides badge operations.
/// </summary>
public class BadgeService : IBadgeService
{
    private readonly MovieAppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public BadgeService(MovieAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<List<Badge>> GetUserBadges(int userId)
    {
        return await _dbContext.UserBadges
            .AsNoTracking()
            .Where(userBadge => userBadge.UserId == userId)
            .Include(userBadge => userBadge.Badge)
            .Select(userBadge => userBadge.Badge!)
            .OrderBy(badge => badge.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Badge>> GetAllBadges()
    {
        return await _dbContext.Badges
            .AsNoTracking()
            .OrderBy(badge => badge.BadgeId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<BadgeProgress>> GetBadgeProgress(int userId)
    {
        var reviews = await _dbContext.Reviews
            .Include(review => review.Movie)
            .Where(review => review.UserId == userId)
            .ToListAsync();

        var unlockedBadgeIds = await _dbContext.UserBadges
            .Where(userBadge => userBadge.UserId == userId)
            .Select(userBadge => userBadge.BadgeId)
            .ToListAsync();

        var badges = await _dbContext.Badges
            .AsNoTracking()
            .OrderBy(badge => badge.BadgeId)
            .ToListAsync();

        var distinctReviewedFilms = reviews.Select(review => review.MovieId).Distinct().Count();
        var extraReviews = reviews.Count(review => review.IsExtraReview);
        var fullyCompletedExtraReviews = reviews.Count(IsSuperSeriousReview);
        var comedyReviews = reviews.Count(review => review.Movie?.Genre.Contains("Comedy", StringComparison.OrdinalIgnoreCase) == true);
        var comedyPercentage = reviews.Count == 0 ? 0 : (double)comedyReviews / reviews.Count * 100;

        return badges.Select(badge => CreateBadgeProgress(
            badge,
            unlockedBadgeIds.Contains(badge.BadgeId),
            distinctReviewedFilms,
            extraReviews,
            fullyCompletedExtraReviews,
            comedyPercentage)).ToList();
    }

    /// <inheritdoc />
    public async Task CheckAndAwardBadges(int userId)
    {
        var reviews = await _dbContext.Reviews
            .Include(review => review.Movie)
            .Where(review => review.UserId == userId)
            .ToListAsync();

        var existingBadgeIds = await _dbContext.UserBadges
            .Where(userBadge => userBadge.UserId == userId)
            .Select(userBadge => userBadge.BadgeId)
            .ToListAsync();

        var badges = await _dbContext.Badges.ToListAsync();
        var distinctReviewedFilms = reviews.Select(review => review.MovieId).Distinct().Count();
        var extraReviews = reviews.Count(review => review.IsExtraReview);
        var fullyCompletedExtraReviews = reviews.Count(IsSuperSeriousReview);
        var comedyReviews = reviews.Count(review => review.Movie?.Genre.Contains("Comedy", StringComparison.OrdinalIgnoreCase) == true);
        var comedyPercentage = reviews.Count == 0 ? 0 : (double)comedyReviews / reviews.Count * 100;

        foreach (var badge in badges)
        {
            if (existingBadgeIds.Contains(badge.BadgeId))
            {
                continue;
            }

            var shouldAward = badge.Name switch
            {
                "The Snob" => extraReviews >= badge.CriteriaValue,
                "The Super Serious" => fullyCompletedExtraReviews >= badge.CriteriaValue,
                "Why So Serious" => fullyCompletedExtraReviews >= badge.CriteriaValue,
                "The Joker" => comedyPercentage > badge.CriteriaValue,
                "The Godfather I" => distinctReviewedFilms >= badge.CriteriaValue,
                "The Godfather II" => distinctReviewedFilms >= badge.CriteriaValue,
                "The Godfather III" => distinctReviewedFilms >= badge.CriteriaValue,
                _ => false,
            };

            if (!shouldAward)
            {
                continue;
            }

            await _dbContext.UserBadges.AddAsync(new UserBadge
            {
                UserId = userId,
                BadgeId = badge.BadgeId,
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    private static bool IsSuperSeriousReview(Review review)
    {
        return review.IsExtraReview
            && !string.IsNullOrWhiteSpace(review.ExtraReviewContent)
            && review.CinematographyRating.HasValue
            && review.ActingRating.HasValue
            && review.CgiRating.HasValue
            && review.PlotRating.HasValue
            && review.SoundRating.HasValue
            && !string.IsNullOrWhiteSpace(review.CinematographyText)
            && !string.IsNullOrWhiteSpace(review.ActingText)
            && !string.IsNullOrWhiteSpace(review.CgiText)
            && !string.IsNullOrWhiteSpace(review.PlotText)
            && !string.IsNullOrWhiteSpace(review.SoundText);
    }

    private static BadgeProgress CreateBadgeProgress(
        Badge badge,
        bool isUnlocked,
        int distinctReviewedFilms,
        int extraReviews,
        int fullyCompletedExtraReviews,
        double comedyPercentage)
    {
        return badge.Name switch
        {
            "The Snob" => new BadgeProgress
            {
                BadgeId = badge.BadgeId,
                Name = badge.Name,
                CriteriaValue = badge.CriteriaValue,
                IsUnlocked = isUnlocked,
                RequirementText = $"Write {badge.CriteriaValue} extra reviews.",
                ProgressText = $"Progress: {extraReviews}/{badge.CriteriaValue} extra reviews",
            },
            "The Super Serious" or "Why So Serious" => new BadgeProgress
            {
                BadgeId = badge.BadgeId,
                Name = "Why So Serious",
                CriteriaValue = badge.CriteriaValue,
                IsUnlocked = isUnlocked,
                RequirementText = $"Complete all extra-review fields for {badge.CriteriaValue} films.",
                ProgressText = $"Progress: {fullyCompletedExtraReviews}/{badge.CriteriaValue} fully completed extra reviews",
            },
            "The Joker" => new BadgeProgress
            {
                BadgeId = badge.BadgeId,
                Name = badge.Name,
                CriteriaValue = badge.CriteriaValue,
                IsUnlocked = isUnlocked,
                RequirementText = $"Have more than {badge.CriteriaValue}% comedy reviews.",
                ProgressText = $"Progress: {comedyPercentage:F1}% comedy reviews",
            },
            "The Godfather I" or "The Godfather II" or "The Godfather III" => new BadgeProgress
            {
                BadgeId = badge.BadgeId,
                Name = badge.Name,
                CriteriaValue = badge.CriteriaValue,
                IsUnlocked = isUnlocked,
                RequirementText = $"Review {badge.CriteriaValue} different films.",
                ProgressText = $"Progress: {distinctReviewedFilms}/{badge.CriteriaValue} reviewed films",
            },
            _ => new BadgeProgress
            {
                BadgeId = badge.BadgeId,
                Name = badge.Name,
                CriteriaValue = badge.CriteriaValue,
                IsUnlocked = isUnlocked,
                RequirementText = $"Reach the criteria value of {badge.CriteriaValue}.",
                ProgressText = "Progress unavailable",
            },
        };
    }
}
