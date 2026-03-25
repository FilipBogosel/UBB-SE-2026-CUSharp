#nullable enable

using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Tests.Services;

public class BadgeServiceTests
{
    [Fact]
    public async Task CheckAndAwardBadges_AwardsEligibleBadge()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(CheckAndAwardBadges_AwardsEligibleBadge));
        context.Users.Add(new User { UserId = 1 });
        context.Badges.Add(new Badge { BadgeId = 1, Name = "The Snob", CriteriaValue = 10 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.0 });

        for (var index = 0; index < 10; index++)
        {
            context.Reviews.Add(new Review
            {
                ReviewId = index + 1,
                UserId = 1,
                MovieId = 1,
                StarRating = 4.0f,
                Content = $"Review {index}",
                CreatedAt = DateTime.UtcNow.AddDays(-index),
                IsExtraReview = true,
                ExtraReviewContent = new string('x', 500),
                CinematographyRating = 4.0f,
                CinematographyText = new string('a', 60),
                ActingRating = 4.0f,
                ActingText = new string('b', 60),
                CgiRating = 4.0f,
                CgiText = new string('c', 60),
                PlotRating = 4.0f,
                PlotText = new string('d', 60),
                SoundRating = 4.0f,
                SoundText = new string('e', 60),
            });
        }

        await context.SaveChangesAsync();
        var service = new BadgeService(context);

        await service.CheckAndAwardBadges(1);

        Assert.Single(context.UserBadges);
        Assert.Equal(1, context.UserBadges.Single().BadgeId);
    }

    [Fact]
    public async Task CheckAndAwardBadges_DoesNotAwardDuplicateBadge()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(CheckAndAwardBadges_DoesNotAwardDuplicateBadge));
        context.Users.Add(new User { UserId = 1 });
        context.Badges.Add(new Badge { BadgeId = 1, Name = "The Snob", CriteriaValue = 1 });
        context.Movies.Add(new Movie { MovieId = 1, Title = "Movie A", Genre = "Drama", PosterUrl = "poster", Year = 2001, AverageRating = 4.0 });
        context.Reviews.Add(new Review
        {
            UserId = 1,
            MovieId = 1,
            StarRating = 4.0f,
            Content = "Review",
            CreatedAt = DateTime.UtcNow,
            IsExtraReview = true,
            ExtraReviewContent = new string('x', 500),
            CinematographyRating = 4.0f,
            CinematographyText = new string('a', 60),
            ActingRating = 4.0f,
            ActingText = new string('b', 60),
            CgiRating = 4.0f,
            CgiText = new string('c', 60),
            PlotRating = 4.0f,
            PlotText = new string('d', 60),
            SoundRating = 4.0f,
            SoundText = new string('e', 60),
        });
        await context.SaveChangesAsync();

        var service = new BadgeService(context);

        await service.CheckAndAwardBadges(1);
        await service.CheckAndAwardBadges(1);

        Assert.Single(context.UserBadges);
    }
}
