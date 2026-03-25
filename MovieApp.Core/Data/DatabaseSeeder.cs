#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;

namespace MovieApp.Core.Data;

/// <summary>
/// Seeds the application database with initial data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database if it is empty.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public static async Task SeedAsync(MovieAppDbContext dbContext)
    {
        if (await dbContext.Movies.AnyAsync())
        {
            return;
        }

        var movies = new List<Movie>
        {
            new() { Title = "Inception", Year = 2010, PosterUrl = "https://m.media-amazon.com/images/I/51s+4a5NL4L._AC_.jpg", Genre = "Sci-Fi", AverageRating = 0 },
            new() { Title = "The Grand Budapest Hotel", Year = 2014, PosterUrl = "https://m.media-amazon.com/images/I/81A-mvlo+QL._AC_SL1500_.jpg", Genre = "Comedy", AverageRating = 0 },
            new() { Title = "Parasite", Year = 2019, PosterUrl = "https://m.media-amazon.com/images/I/91a4rAqWzKL._AC_SL1500_.jpg", Genre = "Thriller", AverageRating = 0 },
            new() { Title = "The Dark Knight", Year = 2008, PosterUrl = "https://m.media-amazon.com/images/I/51EbJjlLgIL._AC_.jpg", Genre = "Action", AverageRating = 0 },
            new() { Title = "Arrival", Year = 2016, PosterUrl = "https://m.media-amazon.com/images/I/81xqN8L0H8L._AC_SL1500_.jpg", Genre = "Sci-Fi", AverageRating = 0 },
            new() { Title = "La La Land", Year = 2016, PosterUrl = "https://m.media-amazon.com/images/I/81w3bC6a7SL._AC_SL1500_.jpg", Genre = "Musical", AverageRating = 0 },
            new() { Title = "Knives Out", Year = 2019, PosterUrl = "https://m.media-amazon.com/images/I/81h4jS9yTRL._AC_SL1500_.jpg", Genre = "Comedy", AverageRating = 0 },
            new() { Title = "Mad Max: Fury Road", Year = 2015, PosterUrl = "https://m.media-amazon.com/images/I/81fC9bD8d9L._AC_SL1500_.jpg", Genre = "Action", AverageRating = 0 },
            new() { Title = "Soul", Year = 2020, PosterUrl = "https://m.media-amazon.com/images/I/81cHiy4+Q4L._AC_SL1500_.jpg", Genre = "Animation", AverageRating = 0 },
            new() { Title = "The Social Network", Year = 2010, PosterUrl = "https://m.media-amazon.com/images/I/81K7Pj4YduL._AC_SL1500_.jpg", Genre = "Drama", AverageRating = 0 },
        };

        var users = new List<User>
        {
            new() { UserId = 1 },
            new() { UserId = 2 },
            new() { UserId = 3 },
        };

        var badges = new List<Badge>
        {
            new() { Name = "The Snob", CriteriaValue = 10 },
            new() { Name = "Why So Serious", CriteriaValue = 50 },
            new() { Name = "The Joker", CriteriaValue = 70 },
            new() { Name = "The Godfather I", CriteriaValue = 100 },
            new() { Name = "The Godfather II", CriteriaValue = 200 },
            new() { Name = "The Godfather III", CriteriaValue = 300 },
        };

        await dbContext.Movies.AddRangeAsync(movies);
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.Badges.AddRangeAsync(badges);
        await dbContext.SaveChangesAsync();

        var reviews = new List<Review>
        {
            new() { UserId = 1, MovieId = movies[0].MovieId, StarRating = 4.5f, Content = "Inventive and emotionally grounded sci-fi.", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { UserId = 2, MovieId = movies[0].MovieId, StarRating = 4.0f, Content = "A puzzle box that rewards attention.", CreatedAt = DateTime.UtcNow.AddDays(-13) },
            new() { UserId = 3, MovieId = movies[1].MovieId, StarRating = 4.5f, Content = "Meticulous comedy with delightful performances.", CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { UserId = 1, MovieId = movies[2].MovieId, StarRating = 5.0f, Content = "Sharp social commentary and suspense.", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { UserId = 2, MovieId = movies[3].MovieId, StarRating = 5.0f, Content = "A benchmark for blockbuster craft.", CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new() { UserId = 3, MovieId = movies[4].MovieId, StarRating = 4.0f, Content = "Thoughtful first contact story.", CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { UserId = 1, MovieId = movies[6].MovieId, StarRating = 4.5f, Content = "A playful whodunit with real bite.", CreatedAt = DateTime.UtcNow.AddDays(-6) },
            new() { UserId = 2, MovieId = movies[7].MovieId, StarRating = 4.5f, Content = "Pure momentum and visual brilliance.", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = 3, MovieId = movies[8].MovieId, StarRating = 4.0f, Content = "Warm, reflective, and funny.", CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new() { UserId = 1, MovieId = movies[9].MovieId, StarRating = 4.0f, Content = "A tense portrait of ambition and ego.", CreatedAt = DateTime.UtcNow.AddDays(-3) },
        };

        await dbContext.Reviews.AddRangeAsync(reviews);

        await dbContext.UserStats.AddRangeAsync(
            new UserStats { UserId = 1, TotalPoints = 18, WeeklyScore = 6 },
            new UserStats { UserId = 2, TotalPoints = 12, WeeklyScore = 4 },
            new UserStats { UserId = 3, TotalPoints = 10, WeeklyScore = 3 });

        await dbContext.Comments.AddRangeAsync(
            new Comment { AuthorId = 1, MovieId = movies[0].MovieId, Content = "Still one of the best endings in modern sci-fi.", CreatedAt = DateTime.UtcNow.AddHours(-12) },
            new Comment { AuthorId = 2, MovieId = movies[0].MovieId, Content = "The rotating hallway scene never gets old.", CreatedAt = DateTime.UtcNow.AddHours(-8) },
            new Comment { AuthorId = 3, MovieId = movies[2].MovieId, Content = "The tonal shifts here are unbelievable.", CreatedAt = DateTime.UtcNow.AddHours(-5) });

        await dbContext.SaveChangesAsync();

        foreach (var movie in movies)
        {
            movie.AverageRating = await dbContext.Reviews
                .Where(review => review.MovieId == movie.MovieId)
                .Select(review => (double?)review.StarRating)
                .AverageAsync() ?? 0;
        }

        var battleCandidates = movies
            .Where(movie => movie.Title is "Arrival" or "Soul")
            .ToList();

        var monday = GetMonday(DateTime.UtcNow);
        await dbContext.Battles.AddAsync(new Battle
        {
            FirstMovieId = battleCandidates[0].MovieId,
            SecondMovieId = battleCandidates[1].MovieId,
            InitialRatingFirstMovie = battleCandidates[0].AverageRating,
            InitialRatingSecondMovie = battleCandidates[1].AverageRating,
            StartDate = monday,
            EndDate = monday.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59),
            Status = "Active",
        });

        await dbContext.SaveChangesAsync();
    }

    private static DateTime GetMonday(DateTime current)
    {
        var dayOfWeek = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
        return current.Date.AddDays(-dayOfWeek);
    }
}
