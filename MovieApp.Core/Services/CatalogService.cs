#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides catalog operations for movies.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly MovieAppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CatalogService(MovieAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<List<Movie>> GetAllMovies()
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .OrderBy(movie => movie.Title)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Movie?> GetMovieById(int movieId)
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(movie => movie.MovieId == movieId);
    }

    /// <inheritdoc />
    public async Task<List<Movie>> SearchMovies(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllMovies();
        }

        var loweredQuery = query.Trim().ToLowerInvariant();
        return await _dbContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Title.ToLower().Contains(loweredQuery))
            .OrderBy(movie => movie.Title)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Movie>> FilterMovies(string? genre, float minRating)
    {
        var query = _dbContext.Movies
            .AsNoTracking()
            .Where(movie => movie.AverageRating >= minRating);

        if (!string.IsNullOrWhiteSpace(genre) && !string.Equals(genre, "All", StringComparison.OrdinalIgnoreCase))
        {
            var loweredGenre = genre.Trim().ToLowerInvariant();
            query = query.Where(movie => movie.Genre.ToLower() == loweredGenre);
        }

        return await query.OrderBy(movie => movie.Title).ToListAsync();
    }
}
