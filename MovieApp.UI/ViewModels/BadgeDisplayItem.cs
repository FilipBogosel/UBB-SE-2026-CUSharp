#nullable enable

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Represents a badge row for display in the UI.
/// </summary>
public class BadgeDisplayItem
{
    public string Name { get; set; } = string.Empty;

    public int CriteriaValue { get; set; }

    public bool IsUnlocked { get; set; }

    public string RequirementText { get; set; } = string.Empty;

    public string ProgressText { get; set; } = string.Empty;

    public string StatusLabel => IsUnlocked ? "Unlocked" : "Locked";
}
