#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides gamification point operations.
/// </summary>
public class PointService : IPointService
{
    private readonly MovieAppDbContext _dbContext;
    private readonly IBadgeService _badgeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="badgeService">The badge service.</param>
    public PointService(MovieAppDbContext dbContext, IBadgeService badgeService)
    {
        _dbContext = dbContext;
        _badgeService = badgeService;
    }

    /// <inheritdoc />
    public async Task<UserStats> GetUserStats(int userId)
    {
        return await GetOrCreateUserStats(userId);
    }

    /// <inheritdoc />
    public async Task AddPoints(int userId, int movieId, bool isBattleMovie)
    {
        var userStats = await GetOrCreateUserStats(userId);
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(item => item.MovieId == movieId)
            ?? throw new InvalidOperationException("Movie not found.");

        var pointsToAdd = 0;
        if (movie.AverageRating > 3.5)
        {
            pointsToAdd += 2;
        }
        else if (movie.AverageRating < 2.0)
        {
            pointsToAdd += 1;
        }

        if (isBattleMovie)
        {
            pointsToAdd += 5;
        }

        userStats.TotalPoints = Math.Max(0, userStats.TotalPoints + pointsToAdd);
        userStats.WeeklyScore = Math.Max(0, userStats.WeeklyScore + pointsToAdd);

        await _dbContext.SaveChangesAsync();
        await _badgeService.CheckAndAwardBadges(userId);
    }

    /// <inheritdoc />
    public async Task DeductPoints(int userId, int points)
    {
        if (points < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points), "Points cannot be negative.");
        }

        var userStats = await GetOrCreateUserStats(userId);
        userStats.TotalPoints = Math.Max(0, userStats.TotalPoints - points);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task FreezePoints(int userId, int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        var userStats = await GetOrCreateUserStats(userId);
        if (userStats.TotalPoints < amount)
        {
            throw new InvalidOperationException("Insufficient balance to freeze points.");
        }

        userStats.TotalPoints -= amount;
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RefundPoints(int userId, int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        var userStats = await GetOrCreateUserStats(userId);
        userStats.TotalPoints += amount;
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateWeeklyScore(int userId)
    {
        var userStats = await GetOrCreateUserStats(userId);
        var weekStart = GetMonday(DateTime.UtcNow);
        var weekEnd = weekStart.AddDays(7);

        var reviews = await _dbContext.Reviews
            .Include(review => review.Movie)
            .Where(review => review.UserId == userId && review.CreatedAt >= weekStart && review.CreatedAt < weekEnd)
            .ToListAsync();

        var battleMovieIds = await _dbContext.Battles
            .Where(battle => battle.StartDate < weekEnd && battle.EndDate >= weekStart)
            .Select(battle => battle.FirstMovieId)
            .Concat(_dbContext.Battles
                .Where(battle => battle.StartDate < weekEnd && battle.EndDate >= weekStart)
                .Select(battle => battle.SecondMovieId))
            .Distinct()
            .ToListAsync();

        var weeklyScore = 0;
        foreach (var review in reviews)
        {
            if (review.Movie?.AverageRating > 3.5)
            {
                weeklyScore += 2;
            }
            else if (review.Movie?.AverageRating < 2.0)
            {
                weeklyScore += 1;
            }

            if (battleMovieIds.Contains(review.MovieId))
            {
                weeklyScore += 5;
            }
        }

        userStats.WeeklyScore = Math.Max(0, weeklyScore);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<UserStats> GetOrCreateUserStats(int userId)
    {
        if (!await _dbContext.Users.AnyAsync(user => user.UserId == userId))
        {
            throw new InvalidOperationException("User not found.");
        }

        var stats = await _dbContext.UserStats.FirstOrDefaultAsync(item => item.UserId == userId);
        if (stats is not null)
        {
            return stats;
        }

        stats = new UserStats
        {
            UserId = userId,
            TotalPoints = 0,
            WeeklyScore = 0,
        };

        await _dbContext.UserStats.AddAsync(stats);
        await _dbContext.SaveChangesAsync();
        return stats;
    }

    private static DateTime GetMonday(DateTime current)
    {
        var offset = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
        return current.Date.AddDays(-offset);
    }
}
