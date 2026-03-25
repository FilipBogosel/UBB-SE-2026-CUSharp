#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes catalog operations for movies.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Gets all movies in the catalog.
    /// </summary>
    Task<List<Movie>> GetAllMovies();

    /// <summary>
    /// Gets a movie by its identifier.
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    Task<Movie?> GetMovieById(int movieId);

    /// <summary>
    /// Searches movies by title.
    /// </summary>
    /// <param name="query">The query text.</param>
    Task<List<Movie>> SearchMovies(string query);

    /// <summary>
    /// Filters movies by genre and minimum rating.
    /// </summary>
    /// <param name="genre">The genre to filter by.</param>
    /// <param name="minRating">The minimum allowed rating.</param>
    Task<List<Movie>> FilterMovies(string? genre, float minRating);
}
