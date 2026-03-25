#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents an achievement badge.
/// </summary>
public class Badge
{
    public int BadgeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CriteriaValue { get; set; }

    public ICollection<UserBadge> UserBadges { get; set; } = new HashSet<UserBadge>();
}
