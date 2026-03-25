#nullable enable

using MovieApp.Core.Models;
using MovieApp.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Represents a comment node in the forum tree.
/// </summary>
public class CommentNodeViewModel : ViewModelBase
{
    private readonly Func<int, string, Task> _submitReplyAsync;
    private readonly Func<int, Task> _deleteAsync;
    private bool _isReplyEditorVisible;
    private string _replyText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentNodeViewModel"/> class.
    /// </summary>
    /// <param name="comment">The comment model.</param>
    /// <param name="submitReplyAsync">The reply callback.</param>
    /// <param name="deleteAsync">The delete callback.</param>
    public CommentNodeViewModel(Comment comment, Func<int, string, Task> submitReplyAsync, Func<int, Task> deleteAsync)
    {
        CommentId = comment.MessageId;
        AuthorId = comment.AuthorId;
        Content = comment.Content;
        CreatedAt = comment.CreatedAt;
        ParentCommentId = comment.ParentCommentId;
        MovieId = comment.MovieId;
        _submitReplyAsync = submitReplyAsync;
        _deleteAsync = deleteAsync;

        ToggleReplyCommand = new RelayCommand(_ => IsReplyEditorVisible = !IsReplyEditorVisible);
        SubmitReplyCommand = new AsyncRelayCommand(_ => SubmitReplyInternalAsync(), _ => !string.IsNullOrWhiteSpace(ReplyText));
        DeleteCommand = new AsyncRelayCommand(_ => _deleteAsync(CommentId));
    }

    public int CommentId { get; }

    public int AuthorId { get; }

    public int MovieId { get; }

    public int? ParentCommentId { get; }

    public string Content { get; }

    public DateTime CreatedAt { get; }

    public string AuthorLabel => $"User #{AuthorId}";

    public ObservableCollection<CommentNodeViewModel> Replies { get; } = [];

    public bool IsReplyEditorVisible
    {
        get => _isReplyEditorVisible;
        set => SetProperty(ref _isReplyEditorVisible, value);
    }

    public string ReplyText
    {
        get => _replyText;
        set
        {
            if (SetProperty(ref _replyText, value))
            {
                (SubmitReplyCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ToggleReplyCommand { get; }

    public ICommand SubmitReplyCommand { get; }

    public ICommand DeleteCommand { get; }

    private async Task SubmitReplyInternalAsync()
    {
        await _submitReplyAsync(CommentId, ReplyText);
        ReplyText = string.Empty;
        IsReplyEditorVisible = false;
    }
}
