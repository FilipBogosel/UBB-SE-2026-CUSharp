#nullable enable

using Moq;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests.Services;

public class BattleServiceTests
{
    [Fact]
    public async Task CreateBattle_WithLargeRatingDifference_Throws()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(CreateBattle_WithLargeRatingDifference_Throws));
        context.Movies.AddRange(
            new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.5 },
            new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", PosterUrl = "poster", Year = 2002, AverageRating = 3.0 });
        await context.SaveChangesAsync();

        var service = new BattleService(context, Mock.Of<IPointService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBattle(1, 2));
    }

    [Fact]
    public async Task PlaceBet_DuplicateBet_Throws()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(PlaceBet_DuplicateBet_Throws));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.AddRange(
            new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.0 },
            new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", PosterUrl = "poster", Year = 2002, AverageRating = 4.1 });
        context.Battles.Add(new Battle
        {
            BattleId = 10,
            FirstMovieId = 1,
            SecondMovieId = 2,
            InitialRatingFirstMovie = 4.0,
            InitialRatingSecondMovie = 4.1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = "Active",
        });
        context.Bets.Add(new Bet { UserId = 1, BattleId = 10, MovieId = 1, Amount = 3 });
        await context.SaveChangesAsync();

        var pointServiceMock = new Mock<IPointService>();
        var service = new BattleService(context, pointServiceMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBet(1, 10, 2, 4));
    }

    [Fact]
    public async Task DetermineWinner_ReturnsMovieWithHigherDelta()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(DetermineWinner_ReturnsMovieWithHigherDelta));
        context.Movies.AddRange(
            new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.6 },
            new Movie { MovieId = 2, Title = "Movie B", Genre = "Drama", PosterUrl = "poster", Year = 2002, AverageRating = 4.4 });
        context.Battles.Add(new Battle
        {
            BattleId = 12,
            FirstMovieId = 1,
            SecondMovieId = 2,
            InitialRatingFirstMovie = 4.1,
            InitialRatingSecondMovie = 4.3,
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(4),
            Status = "Active",
        });
        await context.SaveChangesAsync();

        var service = new BattleService(context, Mock.Of<IPointService>());

        var winnerMovieId = await service.DetermineWinner(12);

        Assert.Equal(1, winnerMovieId);
    }
}
