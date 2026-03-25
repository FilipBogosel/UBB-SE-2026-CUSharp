#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.UI.Commands;
using MovieApp.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Drives the movie detail experience.
/// </summary>
public class MovieDetailViewModel : ViewModelBase
{
    private const int LoggedInUserId = 1;

    private readonly ICatalogService _catalogService;
    private readonly IReviewService _reviewService;
    private readonly ExternalReviewService _externalReviewService;
    private readonly MovieSelectionService _movieSelectionService;
    private readonly GamificationRefreshService _gamificationRefreshService;
    private Movie? _currentMovie;
    private string _statusMessage = "Select a movie to view details.";
    private float _selectedRating = 4.0f;
    private string _reviewContent = string.Empty;
    private bool _isExtraReviewExpanded;
    private string _extraReviewContent = string.Empty;
    private float _selectedCinematographyRating = 4.0f;
    private float _selectedActingRating = 4.0f;
    private float _selectedCgiRating = 4.0f;
    private float _selectedPlotRating = 4.0f;
    private float _selectedSoundRating = 4.0f;
    private string _cinematographyText = string.Empty;
    private string _actingText = string.Empty;
    private string _cgiText = string.Empty;
    private string _plotText = string.Empty;
    private string _soundText = string.Empty;
    private double _criticScore;
    private double _audienceScore;
    private bool _isLoadingExternalData;
    private bool _isPolarized;
    private int? _currentUserReviewId;
    private bool _hasUserReview;
    private bool _hasSubmittedExtraReview;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovieDetailViewModel"/> class.
    /// </summary>
    public MovieDetailViewModel(
        ICatalogService catalogService,
        IReviewService reviewService,
        ExternalReviewService externalReviewService,
        MovieSelectionService movieSelectionService,
        GamificationRefreshService gamificationRefreshService)
    {
        _catalogService = catalogService;
        _reviewService = reviewService;
        _externalReviewService = externalReviewService;
        _movieSelectionService = movieSelectionService;
        _gamificationRefreshService = gamificationRefreshService;
        _movieSelectionService.SelectedMovieChanged += OnSelectedMovieChanged;

        RatingOptions =
        [
            0.0f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f,
        ];

        SubmitReviewCommand = new AsyncRelayCommand(_ => SubmitReviewAsync(), _ => HasSelectedMovie && !HasUserReview);
        ToggleExtraReviewCommand = new RelayCommand(_ => IsExtraReviewExpanded = !IsExtraReviewExpanded, _ => HasUserReview);
        SubmitExtraReviewCommand = new AsyncRelayCommand(_ => SubmitExtraReviewAsync(), _ => HasUserReview && !_hasSubmittedExtraReview);
    }

    public Movie? CurrentMovie
    {
        get => _currentMovie;
        private set
        {
            if (SetProperty(ref _currentMovie, value))
            {
                OnPropertyChanged(nameof(HasSelectedMovie));
                RaiseCommandStates();
            }
        }
    }

    public bool HasSelectedMovie => CurrentMovie is not null;

    public ObservableCollection<Review> Reviews { get; } = [];

    public ObservableCollection<CriticReview> CriticReviews { get; } = [];

    public ObservableCollection<LexiconWordItem> LexiconWords { get; } = [];

    public ObservableCollection<float> RatingOptions { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public float SelectedRating
    {
        get => _selectedRating;
        set => SetProperty(ref _selectedRating, value);
    }

    public string ReviewContent
    {
        get => _reviewContent;
        set => SetProperty(ref _reviewContent, value);
    }

    public bool HasUserReview
    {
        get => _hasUserReview;
        private set
        {
            if (SetProperty(ref _hasUserReview, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IsExtraReviewExpanded
    {
        get => _isExtraReviewExpanded;
        set => SetProperty(ref _isExtraReviewExpanded, value);
    }

    public string ExtraReviewContent
    {
        get => _extraReviewContent;
        set => SetProperty(ref _extraReviewContent, value);
    }

    public float SelectedCinematographyRating
    {
        get => _selectedCinematographyRating;
        set => SetProperty(ref _selectedCinematographyRating, value);
    }

    public float SelectedActingRating
    {
        get => _selectedActingRating;
        set => SetProperty(ref _selectedActingRating, value);
    }

    public float SelectedCgiRating
    {
        get => _selectedCgiRating;
        set => SetProperty(ref _selectedCgiRating, value);
    }

    public float SelectedPlotRating
    {
        get => _selectedPlotRating;
        set => SetProperty(ref _selectedPlotRating, value);
    }

    public float SelectedSoundRating
    {
        get => _selectedSoundRating;
        set => SetProperty(ref _selectedSoundRating, value);
    }

    public string CinematographyText
    {
        get => _cinematographyText;
        set => SetProperty(ref _cinematographyText, value);
    }

    public string ActingText
    {
        get => _actingText;
        set => SetProperty(ref _actingText, value);
    }

    public string CgiText
    {
        get => _cgiText;
        set => SetProperty(ref _cgiText, value);
    }

    public string PlotText
    {
        get => _plotText;
        set => SetProperty(ref _plotText, value);
    }

    public string SoundText
    {
        get => _soundText;
        set => SetProperty(ref _soundText, value);
    }

    public double CriticScore
    {
        get => _criticScore;
        private set => SetProperty(ref _criticScore, value);
    }

    public double AudienceScore
    {
        get => _audienceScore;
        private set => SetProperty(ref _audienceScore, value);
    }

    public bool IsLoadingExternalData
    {
        get => _isLoadingExternalData;
        private set => SetProperty(ref _isLoadingExternalData, value);
    }

    public bool IsPolarized
    {
        get => _isPolarized;
        private set => SetProperty(ref _isPolarized, value);
    }

    public ICommand SubmitReviewCommand { get; }

    public ICommand ToggleExtraReviewCommand { get; }

    public ICommand SubmitExtraReviewCommand { get; }

    private void OnSelectedMovieChanged(object? sender, Movie? movie)
    {
        _ = LoadMovieAsync(movie?.MovieId);
    }

    private async Task LoadMovieAsync(int? movieId)
    {
        try
        {
            if (movieId is null)
            {
                CurrentMovie = null;
                Reviews.Clear();
                CriticReviews.Clear();
                LexiconWords.Clear();
                HasUserReview = false;
                _hasSubmittedExtraReview = false;
                _currentUserReviewId = null;
                StatusMessage = "Select a movie to view details.";
                return;
            }

            CurrentMovie = await _catalogService.GetMovieById(movieId.Value);
            if (CurrentMovie is null)
            {
                StatusMessage = "Movie not found.";
                return;
            }

            await LoadReviewsAsync();
            await LoadExternalDataAsync(CurrentMovie.Title);
            StatusMessage = "Movie details loaded.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task LoadReviewsAsync()
    {
        if (CurrentMovie is null)
        {
            return;
        }

        var reviews = await _reviewService.GetReviewsForMovie(CurrentMovie.MovieId);
        Reviews.Clear();
        foreach (var review in reviews)
        {
            Reviews.Add(review);
        }

        var userReview = reviews.FirstOrDefault(review => review.UserId == LoggedInUserId);
        _currentUserReviewId = userReview?.ReviewId;
        _hasSubmittedExtraReview = userReview?.IsExtraReview == true;
        HasUserReview = userReview is not null;
        RaiseCommandStates();

        CurrentMovie.AverageRating = await _reviewService.GetAverageRating(CurrentMovie.MovieId);
        OnPropertyChanged(nameof(CurrentMovie));
    }

    private async Task LoadExternalDataAsync(string movieTitle)
    {
        IsLoadingExternalData = true;
        CriticReviews.Clear();
        LexiconWords.Clear();

        try
        {
            var reviews = await _externalReviewService.GetExternalReviews(movieTitle);
            var aggregateScores = await _externalReviewService.GetAggregateScores(movieTitle);

            foreach (var review in reviews)
            {
                CriticReviews.Add(review);
            }

            foreach (var item in _externalReviewService.AnalyseLexicon(reviews))
            {
                LexiconWords.Add(new LexiconWordItem { Word = item.Word, Count = item.Count });
            }

            CriticScore = aggregateScores.CriticScore;
            AudienceScore = aggregateScores.AudienceScore;
            IsPolarized = _externalReviewService.IsPolarized(CriticScore, AudienceScore);
        }
        finally
        {
            IsLoadingExternalData = false;
        }
    }

    private async Task SubmitReviewAsync()
    {
        if (CurrentMovie is null)
        {
            return;
        }

        try
        {
            await _reviewService.AddReview(LoggedInUserId, CurrentMovie.MovieId, SelectedRating, ReviewContent);
            ReviewContent = string.Empty;
            _gamificationRefreshService.RequestRefresh();
            StatusMessage = "Review submitted successfully.";
            await LoadMovieAsync(CurrentMovie.MovieId);
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task SubmitExtraReviewAsync()
    {
        if (_currentUserReviewId is null)
        {
            return;
        }

        try
        {
            await _reviewService.SubmitExtraReview(
                _currentUserReviewId.Value,
                SelectedCgiRating,
                CgiText,
                SelectedActingRating,
                ActingText,
                SelectedPlotRating,
                PlotText,
                SelectedSoundRating,
                SoundText,
                SelectedCinematographyRating,
                CinematographyText,
                ExtraReviewContent);

            IsExtraReviewExpanded = false;
            _gamificationRefreshService.RequestRefresh();
            StatusMessage = "Extra review submitted successfully.";
            await LoadMovieAsync(CurrentMovie?.MovieId);
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void RaiseCommandStates()
    {
        (SubmitReviewCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ToggleExtraReviewCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SubmitExtraReviewCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
