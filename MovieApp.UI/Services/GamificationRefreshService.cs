#nullable enable

namespace MovieApp.UI.Services;

/// <summary>
/// Broadcasts point and badge refresh requests across the UI.
/// </summary>
public class GamificationRefreshService
{
    /// <summary>
    /// Occurs when gamification data should be refreshed.
    /// </summary>
    public event EventHandler? RefreshRequested;

    /// <summary>
    /// Requests a refresh of point and badge data.
    /// </summary>
    public void RequestRefresh()
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }
}
