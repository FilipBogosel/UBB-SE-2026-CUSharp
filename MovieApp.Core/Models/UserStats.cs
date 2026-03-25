#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents gamification statistics for a user.
/// </summary>
public class UserStats
{
    public int StatsId { get; set; }

    public int UserId { get; set; }

    public int TotalPoints { get; set; }

    public int WeeklyScore { get; set; }

    public User? User { get; set; }
}
