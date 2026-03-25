#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.UI.Commands;
using MovieApp.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Displays current user points, weekly score, and badges.
/// </summary>
public class ProfileViewModel : ViewModelBase
{
    private const int LoggedInUserId = 1;

    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;
    private readonly GamificationRefreshService _refreshService;
    private int _totalPoints;
    private int _weeklyScore;
    private string _statusMessage = "Loading points and badges...";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
    /// </summary>
    public ProfileViewModel(IPointService pointService, IBadgeService badgeService, GamificationRefreshService refreshService)
    {
        _pointService = pointService;
        _badgeService = badgeService;
        _refreshService = refreshService;
        _refreshService.RefreshRequested += OnRefreshRequested;
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    /// <summary>
    /// Gets the total points for the logged-in user.
    /// </summary>
    public int TotalPoints
    {
        get => _totalPoints;
        private set => SetProperty(ref _totalPoints, value);
    }

    /// <summary>
    /// Gets the weekly score for the logged-in user.
    /// </summary>
    public int WeeklyScore
    {
        get => _weeklyScore;
        private set => SetProperty(ref _weeklyScore, value);
    }

    /// <summary>
    /// Gets the profile status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Gets the earned badge count.
    /// </summary>
    public int EarnedBadgeCount => Badges.Count(badge => badge.IsUnlocked);

    /// <summary>
    /// Gets the displayed badges.
    /// </summary>
    public ObservableCollection<BadgeDisplayItem> Badges { get; } = [];

    /// <summary>
    /// Gets the refresh command.
    /// </summary>
    public ICommand RefreshCommand { get; }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            await _pointService.UpdateWeeklyScore(LoggedInUserId);
            var stats = await _pointService.GetUserStats(LoggedInUserId);
            await _badgeService.CheckAndAwardBadges(LoggedInUserId);
            var badgeProgress = await _badgeService.GetBadgeProgress(LoggedInUserId);

            TotalPoints = stats.TotalPoints;
            WeeklyScore = stats.WeeklyScore;

            Badges.Clear();
            foreach (var badge in badgeProgress)
            {
                Badges.Add(new BadgeDisplayItem
                {
                    Name = badge.Name,
                    CriteriaValue = badge.CriteriaValue,
                    IsUnlocked = badge.IsUnlocked,
                    RequirementText = badge.RequirementText,
                    ProgressText = badge.ProgressText,
                });
            }

            OnPropertyChanged(nameof(EarnedBadgeCount));
            StatusMessage = $"{EarnedBadgeCount} badge(s) unlocked.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }
}
