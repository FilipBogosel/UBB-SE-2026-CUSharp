#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.UI.Commands;
using MovieApp.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Drives the battle arena view.
/// </summary>
public class BattleViewModel : ViewModelBase
{
    private const int LoggedInUserId = 1;

    private readonly IBattleService _battleService;
    private readonly IPointService _pointService;
    private readonly GamificationRefreshService _refreshService;
    private Battle? _activeBattle;
    private string _statusMessage = "Loading battle arena...";
    private bool _isBetPanelVisible;
    private string _betAmount = string.Empty;
    private BattleMovieChoice? _selectedMovieChoice;
    private int _currentPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="BattleViewModel"/> class.
    /// </summary>
    public BattleViewModel(IBattleService battleService, IPointService pointService, GamificationRefreshService refreshService)
    {
        _battleService = battleService;
        _pointService = pointService;
        _refreshService = refreshService;
        _refreshService.RefreshRequested += OnRefreshRequested;

        RefreshCommand = new AsyncRelayCommand(_ => LoadBattleAsync());
        ToggleBetPanelCommand = new RelayCommand(_ => IsBetPanelVisible = !IsBetPanelVisible, _ => HasActiveBattle);
        ConfirmBetCommand = new AsyncRelayCommand(_ => ConfirmBetAsync(), _ => HasActiveBattle);

        _ = LoadBattleAsync();
    }

    public Battle? ActiveBattle
    {
        get => _activeBattle;
        private set
        {
            if (SetProperty(ref _activeBattle, value))
            {
                OnPropertyChanged(nameof(HasActiveBattle));
                OnPropertyChanged(nameof(FirstMovie));
                OnPropertyChanged(nameof(SecondMovie));
                (ToggleBetPanelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ConfirmBetCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasActiveBattle => ActiveBattle is not null;

    public Movie? FirstMovie => ActiveBattle?.FirstMovie;

    public Movie? SecondMovie => ActiveBattle?.SecondMovie;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBetPanelVisible
    {
        get => _isBetPanelVisible;
        set => SetProperty(ref _isBetPanelVisible, value);
    }

    public string BetAmount
    {
        get => _betAmount;
        set => SetProperty(ref _betAmount, value);
    }

    public BattleMovieChoice? SelectedMovieChoice
    {
        get => _selectedMovieChoice;
        set => SetProperty(ref _selectedMovieChoice, value);
    }

    public int CurrentPoints
    {
        get => _currentPoints;
        private set => SetProperty(ref _currentPoints, value);
    }

    public ObservableCollection<BattleMovieChoice> MovieChoices { get; } = [];

    public ICommand RefreshCommand { get; }

    public ICommand ToggleBetPanelCommand { get; }

    public ICommand ConfirmBetCommand { get; }

    private async Task LoadBattleAsync()
    {
        try
        {
            ActiveBattle = await _battleService.GetActiveBattle();
            CurrentPoints = (await _pointService.GetUserStats(LoggedInUserId)).TotalPoints;

            MovieChoices.Clear();
            if (ActiveBattle is not null)
            {
                if (ActiveBattle.FirstMovie is not null)
                {
                    MovieChoices.Add(new BattleMovieChoice { MovieId = ActiveBattle.FirstMovie.MovieId, Title = ActiveBattle.FirstMovie.Title });
                }

                if (ActiveBattle.SecondMovie is not null)
                {
                    MovieChoices.Add(new BattleMovieChoice { MovieId = ActiveBattle.SecondMovie.MovieId, Title = ActiveBattle.SecondMovie.Title });
                }

                SelectedMovieChoice = MovieChoices.FirstOrDefault();
                StatusMessage = "An active battle is live this week.";
            }
            else
            {
                IsBetPanelVisible = false;
                StatusMessage = "No active battle this week.";
            }
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task ConfirmBetAsync()
    {
        if (ActiveBattle is null || SelectedMovieChoice is null)
        {
            return;
        }

        if (!int.TryParse(BetAmount, out var amount))
        {
            StatusMessage = "Enter a valid whole number of points.";
            return;
        }

        try
        {
            await _battleService.PlaceBet(LoggedInUserId, ActiveBattle.BattleId, SelectedMovieChoice.MovieId, amount);
            BetAmount = string.Empty;
            IsBetPanelVisible = false;
            _refreshService.RequestRefresh();
            StatusMessage = "Bet placed successfully.";
            await LoadBattleAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadBattleAsync();
    }
}
