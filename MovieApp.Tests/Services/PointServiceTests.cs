#nullable enable

using Microsoft.EntityFrameworkCore;
using Moq;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests.Services;

public class PointServiceTests
{
    [Fact]
    public async Task AddPoints_AppliesPlusTwoRule()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddPoints_AppliesPlusTwoRule));
        context.Users.Add(new User { UserId = 1 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 0, WeeklyScore = 0 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.0 });
        await context.SaveChangesAsync();

        var badgeServiceMock = new Mock<IBadgeService>();
        var service = new PointService(context, badgeServiceMock.Object);

        await service.AddPoints(1, 1, false);

        var stats = await context.UserStats.FirstAsync(item => item.UserId == 1);
        Assert.Equal(2, stats.TotalPoints);
        badgeServiceMock.Verify(service => service.CheckAndAwardBadges(1), Times.Once);
    }

    [Fact]
    public async Task AddPoints_AppliesPlusOneAndBattleBonus()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddPoints_AppliesPlusOneAndBattleBonus));
        context.Users.Add(new User { UserId = 1 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 0, WeeklyScore = 0 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie B", Genre = "Drama", PosterUrl = "poster", Year = 2002, AverageRating = 1.5 });
        await context.SaveChangesAsync();

        var service = new PointService(context, Mock.Of<IBadgeService>());

        await service.AddPoints(1, 1, true);

        var stats = await context.UserStats.FirstAsync(item => item.UserId == 1);
        Assert.Equal(6, stats.TotalPoints);
    }

    [Fact]
    public async Task FreezePoints_WithInsufficientBalance_Throws()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(FreezePoints_WithInsufficientBalance_Throws));
        context.Users.Add(new User { UserId = 1 });
        context.UserStats.Add(new UserStats { UserId = 1, TotalPoints = 3, WeeklyScore = 0 });
        await context.SaveChangesAsync();

        var service = new PointService(context, Mock.Of<IBadgeService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.FreezePoints(1, 10));
    }
}
