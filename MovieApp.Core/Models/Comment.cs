#nullable enable

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a comment or reply in the movie discussion forum.
/// </summary>
public class Comment
{
    public int MessageId { get; set; }

    public int AuthorId { get; set; }

    public int MovieId { get; set; }

    public int? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public User? Author { get; set; }

    public Movie? Movie { get; set; }

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();
}
