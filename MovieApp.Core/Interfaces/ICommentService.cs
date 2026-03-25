#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

/// <summary>
/// Exposes forum comment operations.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Gets all comments for a movie.
    /// </summary>
    /// <param name="movieId">The movie identifier.</param>
    Task<List<Comment>> GetCommentsForMovie(int movieId);

    /// <summary>
    /// Adds a root comment.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="movieId">The movie identifier.</param>
    /// <param name="content">The comment content.</param>
    Task<Comment> AddComment(int userId, int movieId, string content);

    /// <summary>
    /// Adds a reply to an existing comment.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="parentCommentId">The parent comment identifier.</param>
    /// <param name="content">The reply content.</param>
    Task<Comment> AddReply(int userId, int parentCommentId, string content);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    /// <param name="commentId">The comment identifier.</param>
    Task DeleteComment(int commentId);
}
