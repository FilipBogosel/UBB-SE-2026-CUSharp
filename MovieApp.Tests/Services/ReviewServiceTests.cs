#nullable enable

using Moq;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task AddReview_HappyPath_PersistsReviewAndAwardsPoints()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddReview_HappyPath_PersistsReviewAndAwardsPoints));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Arrival", Genre = "Sci-Fi", PosterUrl = "poster", Year = 2016, AverageRating = 4.2 });
        await context.SaveChangesAsync();

        var pointServiceMock = new Mock<IPointService>();
        var badgeServiceMock = new Mock<IBadgeService>();
        var service = new ReviewService(context, pointServiceMock.Object, badgeServiceMock.Object);

        var review = await service.AddReview(1, 1, 4.5f, "Great film.");

        Assert.NotEqual(0, review.ReviewId);
        Assert.Single(context.Reviews);
        pointServiceMock.Verify(service => service.AddPoints(1, 1, false), Times.Once);
    }

    [Fact]
    public async Task AddReview_DuplicateReview_Throws()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddReview_DuplicateReview_Throws));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Inception", Genre = "Sci-Fi", PosterUrl = "poster", Year = 2010, AverageRating = 4.0 });
        context.Reviews.Add(new Review { UserId = 1, MovieId = 1, StarRating = 4.0f, Content = "Existing", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ReviewService(context, Mock.Of<IPointService>(), Mock.Of<IBadgeService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddReview(1, 1, 4.5f, "Another"));
    }

    [Fact]
    public async Task AddReview_InvalidRating_Throws()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddReview_InvalidRating_Throws));
        context.Users.Add(new User { UserId = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Parasite", Genre = "Thriller", PosterUrl = "poster", Year = 2019, AverageRating = 4.0 });
        await context.SaveChangesAsync();

        var service = new ReviewService(context, Mock.Of<IPointService>(), Mock.Of<IBadgeService>());

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddReview(1, 1, 4.3f, "Invalid"));
    }

    [Fact]
    public async Task AddReview_UpdatesAverageRatingCorrectly()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(AddReview_UpdatesAverageRatingCorrectly));
        context.Users.AddRange(new User { UserId = 1 }, new User { UserId = 2 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Soul", Genre = "Animation", PosterUrl = "poster", Year = 2020, AverageRating = 4.0 });
        context.Reviews.Add(new Review { UserId = 2, MovieId = 1, StarRating = 4.0f, Content = "Nice", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ReviewService(context, Mock.Of<IPointService>(), Mock.Of<IBadgeService>());

        await service.AddReview(1, 1, 5.0f, "Excellent");

        var movie = await context.Movies.FindAsync(1);
        Assert.NotNull(movie);
        Assert.Equal(4.5, movie!.AverageRating, 3);
    }
}
