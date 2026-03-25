#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.UI.Services;

/// <summary>
/// Shares the currently selected movie between views.
/// </summary>
public class MovieSelectionService
{
    private Movie? _currentMovie;

    /// <summary>
    /// Occurs when the selected movie changes.
    /// </summary>
    public event EventHandler<Movie?>? SelectedMovieChanged;

    /// <summary>
    /// Gets the currently selected movie.
    /// </summary>
    public Movie? CurrentMovie => _currentMovie;

    /// <summary>
    /// Selects a movie and notifies subscribers.
    /// </summary>
    /// <param name="movie">The selected movie.</param>
    public void SelectMovie(Movie? movie)
    {
        _currentMovie = movie;
        SelectedMovieChanged?.Invoke(this, movie);
    }
}
