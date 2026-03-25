#nullable enable

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Represents a selectable movie in the battle bet UI.
/// </summary>
public class BattleMovieChoice
{
    public int MovieId { get; set; }

    public string Title { get; set; } = string.Empty;
}
