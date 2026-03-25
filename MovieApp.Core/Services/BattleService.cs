#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides weekly battle operations.
/// </summary>
public class BattleService : IBattleService
{
    private readonly MovieAppDbContext _dbContext;
    private readonly IPointService _pointService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BattleService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="pointService">The point service.</param>
    public BattleService(MovieAppDbContext dbContext, IPointService pointService)
    {
        _dbContext = dbContext;
        _pointService = pointService;
    }

    /// <inheritdoc />
    public async Task<Battle?> GetActiveBattle()
    {
        return await _dbContext.Battles
            .Include(battle => battle.FirstMovie)
            .Include(battle => battle.SecondMovie)
            .FirstOrDefaultAsync(battle => battle.Status == "Active");
    }

    /// <inheritdoc />
    public async Task<Battle> CreateBattle(int firstMovieId, int secondMovieId)
    {
        if (firstMovieId == secondMovieId)
        {
            throw new InvalidOperationException("A battle requires two different movies.");
        }

        if (await _dbContext.Battles.AnyAsync(battle => battle.Status == "Active"))
        {
            throw new InvalidOperationException("Another active battle already exists.");
        }

        var firstMovie = await _dbContext.Movies.FirstOrDefaultAsync(movie => movie.MovieId == firstMovieId)
            ?? throw new InvalidOperationException("First movie not found.");
        var secondMovie = await _dbContext.Movies.FirstOrDefaultAsync(movie => movie.MovieId == secondMovieId)
            ?? throw new InvalidOperationException("Second movie not found.");

        if (Math.Abs(firstMovie.AverageRating - secondMovie.AverageRating) > 0.5)
        {
            throw new InvalidOperationException("Movies must start within 0.5 rating points of each other.");
        }

        var monday = GetMonday(DateTime.UtcNow);
        var battle = new Battle
        {
            FirstMovieId = firstMovieId,
            SecondMovieId = secondMovieId,
            InitialRatingFirstMovie = firstMovie.AverageRating,
            InitialRatingSecondMovie = secondMovie.AverageRating,
            StartDate = monday,
            EndDate = monday.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59),
            Status = "Active",
        };

        await _dbContext.Battles.AddAsync(battle);
        await _dbContext.SaveChangesAsync();

        battle.FirstMovie = firstMovie;
        battle.SecondMovie = secondMovie;
        return battle;
    }

    /// <inheritdoc />
    public async Task<Bet> PlaceBet(int userId, int battleId, int movieId, int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Bet amount must be greater than zero.");
        }

        var battle = await _dbContext.Battles.FirstOrDefaultAsync(item => item.BattleId == battleId)
            ?? throw new InvalidOperationException("Battle not found.");

        if (!string.Equals(battle.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Bets can only be placed on active battles.");
        }

        if (movieId != battle.FirstMovieId && movieId != battle.SecondMovieId)
        {
            throw new InvalidOperationException("Selected movie is not part of the battle.");
        }

        if (await _dbContext.Bets.AnyAsync(bet => bet.UserId == userId && bet.BattleId == battleId))
        {
            throw new InvalidOperationException("The user already placed a bet on this battle.");
        }

        await _pointService.FreezePoints(userId, amount);

        var bet = new Bet
        {
            UserId = userId,
            BattleId = battleId,
            MovieId = movieId,
            Amount = amount,
        };

        await _dbContext.Bets.AddAsync(bet);
        await _dbContext.SaveChangesAsync();
        return bet;
    }

    /// <inheritdoc />
    public async Task<Bet?> GetBet(int userId, int battleId)
    {
        return await _dbContext.Bets
            .AsNoTracking()
            .FirstOrDefaultAsync(bet => bet.UserId == userId && bet.BattleId == battleId);
    }

    /// <inheritdoc />
    public async Task<int> DetermineWinner(int battleId)
    {
        var battle = await _dbContext.Battles
            .Include(item => item.FirstMovie)
            .Include(item => item.SecondMovie)
            .FirstOrDefaultAsync(item => item.BattleId == battleId)
            ?? throw new InvalidOperationException("Battle not found.");

        var firstDelta = (battle.FirstMovie?.AverageRating ?? 0) - battle.InitialRatingFirstMovie;
        var secondDelta = (battle.SecondMovie?.AverageRating ?? 0) - battle.InitialRatingSecondMovie;

        return firstDelta >= secondDelta ? battle.FirstMovieId : battle.SecondMovieId;
    }

    /// <inheritdoc />
    public async Task DistributePayouts(int battleId)
    {
        var battle = await _dbContext.Battles.FirstOrDefaultAsync(item => item.BattleId == battleId)
            ?? throw new InvalidOperationException("Battle not found.");

        var winnerMovieId = await DetermineWinner(battleId);
        var winningBets = await _dbContext.Bets.Where(bet => bet.BattleId == battleId && bet.MovieId == winnerMovieId).ToListAsync();

        foreach (var bet in winningBets)
        {
            var userStats = await _dbContext.UserStats.FirstOrDefaultAsync(stats => stats.UserId == bet.UserId);
            if (userStats is null)
            {
                userStats = new UserStats { UserId = bet.UserId, TotalPoints = 0, WeeklyScore = 0 };
                await _dbContext.UserStats.AddAsync(userStats);
            }

            userStats.TotalPoints += bet.Amount * 2;
        }

        battle.Status = "Finished";
        await _dbContext.SaveChangesAsync();
    }

    private static DateTime GetMonday(DateTime current)
    {
        var offset = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
        return current.Date.AddDays(-offset);
    }
}
