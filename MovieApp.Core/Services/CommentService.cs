#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides forum comment operations.
/// </summary>
public class CommentService : ICommentService
{
    private readonly MovieAppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CommentService(MovieAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<List<Comment>> GetCommentsForMovie(int movieId)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(comment => comment.MovieId == movieId)
            .OrderByDescending(comment => comment.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Comment> AddComment(int userId, int movieId, string content)
    {
        ValidateCommentContent(content);
        var comment = new Comment
        {
            AuthorId = userId,
            MovieId = movieId,
            ParentCommentId = null,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Comments.AddAsync(comment);
        await _dbContext.SaveChangesAsync();
        return comment;
    }

    /// <inheritdoc />
    public async Task<Comment> AddReply(int userId, int parentCommentId, string content)
    {
        ValidateCommentContent(content);
        var parentComment = await _dbContext.Comments.FirstOrDefaultAsync(comment => comment.MessageId == parentCommentId)
            ?? throw new InvalidOperationException("Parent comment not found.");

        var reply = new Comment
        {
            AuthorId = userId,
            MovieId = parentComment.MovieId,
            ParentCommentId = parentCommentId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Comments.AddAsync(reply);
        await _dbContext.SaveChangesAsync();
        return reply;
    }

    /// <inheritdoc />
    public async Task DeleteComment(int commentId)
    {
        var comment = await _dbContext.Comments.FirstOrDefaultAsync(item => item.MessageId == commentId)
            ?? throw new InvalidOperationException("Comment not found.");

        var allComments = await _dbContext.Comments.ToListAsync();
        var idsToDelete = new HashSet<int> { commentId };
        var queue = new Queue<int>();
        queue.Enqueue(comment.MessageId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            foreach (var child in allComments.Where(item => item.ParentCommentId == currentId))
            {
                if (idsToDelete.Add(child.MessageId))
                {
                    queue.Enqueue(child.MessageId);
                }
            }
        }

        var commentsToDelete = allComments.Where(item => idsToDelete.Contains(item.MessageId)).ToList();
        _dbContext.Comments.RemoveRange(commentsToDelete);
        await _dbContext.SaveChangesAsync();
    }

    private static void ValidateCommentContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 10000)
        {
            throw new ArgumentException("Comment content must be between 1 and 10000 characters.", nameof(content));
        }
    }
}
