#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.UI.Services;
using System.Collections.ObjectModel;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Drives the catalog browsing experience.
/// </summary>
public class CatalogViewModel : ViewModelBase
{
    private readonly ICatalogService _catalogService;
    private readonly MovieSelectionService _movieSelectionService;
    private string _searchText = string.Empty;
    private string _selectedGenre = "All";
    private double _minimumRating;
    private Movie? _selectedMovie;
    private string _statusMessage = "Loading catalog...";

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogViewModel"/> class.
    /// </summary>
    /// <param name="catalogService">The catalog service.</param>
    /// <param name="movieSelectionService">The shared movie selection service.</param>
    public CatalogViewModel(ICatalogService catalogService, MovieSelectionService movieSelectionService)
    {
        _catalogService = catalogService;
        _movieSelectionService = movieSelectionService;
        _ = LoadCatalogAsync();
    }

    /// <summary>
    /// Gets the movies shown in the catalog.
    /// </summary>
    public ObservableCollection<Movie> Movies { get; } = [];

    /// <summary>
    /// Gets the available genres.
    /// </summary>
    public ObservableCollection<string> Genres { get; } = ["All"];

    /// <summary>
    /// Gets or sets the live search text.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _ = ApplyFiltersAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected genre.
    /// </summary>
    public string SelectedGenre
    {
        get => _selectedGenre;
        set
        {
            if (SetProperty(ref _selectedGenre, value))
            {
                _ = ApplyFiltersAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum rating filter.
    /// </summary>
    public double MinimumRating
    {
        get => _minimumRating;
        set
        {
            if (SetProperty(ref _minimumRating, value))
            {
                _ = ApplyFiltersAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected movie.
    /// </summary>
    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        set
        {
            if (SetProperty(ref _selectedMovie, value))
            {
                _movieSelectionService.SelectMovie(value);
            }
        }
    }

    /// <summary>
    /// Gets the catalog status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadCatalogAsync()
    {
        try
        {
            var movies = await _catalogService.GetAllMovies();
            PopulateGenres(movies);
            await ApplyFiltersAsync(movies);
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task ApplyFiltersAsync(List<Movie>? allMovies = null)
    {
        try
        {
            var sourceMovies = allMovies ?? await _catalogService.GetAllMovies();
            var filteredMovies = sourceMovies
                .Where(movie => string.IsNullOrWhiteSpace(SearchText) || movie.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .Where(movie => string.Equals(SelectedGenre, "All", StringComparison.OrdinalIgnoreCase) || movie.Genre.Equals(SelectedGenre, StringComparison.OrdinalIgnoreCase))
                .Where(movie => movie.AverageRating >= MinimumRating)
                .OrderBy(movie => movie.Title)
                .ToList();

            Movies.Clear();
            foreach (var movie in filteredMovies)
            {
                Movies.Add(movie);
            }

            StatusMessage = Movies.Count == 0 ? "No movies match the current filters." : $"{Movies.Count} movie(s) available.";

            if (SelectedMovie is null || Movies.All(movie => movie.MovieId != SelectedMovie.MovieId))
            {
                SelectedMovie = Movies.FirstOrDefault();
            }
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void PopulateGenres(IEnumerable<Movie> movies)
    {
        Genres.Clear();
        Genres.Add("All");

        foreach (var genre in movies.Select(movie => movie.Genre).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(genre => genre))
        {
            Genres.Add(genre);
        }
    }
}
