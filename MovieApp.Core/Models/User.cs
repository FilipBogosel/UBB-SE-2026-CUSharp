#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents an application user.
/// </summary>
public class User
{
    public int UserId { get; set; }

    public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();

    public UserStats? UserStats { get; set; }

    public ICollection<UserBadge> UserBadges { get; set; } = new HashSet<UserBadge>();
}
