#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a user's progress toward a badge.
/// </summary>
public class BadgeProgress
{
    public int BadgeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CriteriaValue { get; set; }

    public bool IsUnlocked { get; set; }

    public string RequirementText { get; set; } = string.Empty;

    public string ProgressText { get; set; } = string.Empty;
}
