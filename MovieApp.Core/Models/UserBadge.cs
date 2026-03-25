#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents the many-to-many association between users and badges.
/// </summary>
public class UserBadge
{
    public int UserId { get; set; }

    public int BadgeId { get; set; }

    public User? User { get; set; }

    public Badge? Badge { get; set; }
}
