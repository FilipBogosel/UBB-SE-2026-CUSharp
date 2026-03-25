#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Models;
using MovieApp.UI.Commands;
using MovieApp.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Drives the forum and nested comment display.
/// </summary>
public class ForumViewModel : ViewModelBase
{
    private const int LoggedInUserId = 1;

    private readonly ICommentService _commentService;
    private readonly MovieSelectionService _movieSelectionService;
    private Movie? _selectedMovie;
    private string _newCommentText = string.Empty;
    private string _statusMessage = "Select a movie to view its discussion.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ForumViewModel"/> class.
    /// </summary>
    public ForumViewModel(ICommentService commentService, MovieSelectionService movieSelectionService)
    {
        _commentService = commentService;
        _movieSelectionService = movieSelectionService;
        _movieSelectionService.SelectedMovieChanged += OnSelectedMovieChanged;
        AddCommentCommand = new AsyncRelayCommand(_ => AddCommentAsync(), _ => HasSelectedMovie && !string.IsNullOrWhiteSpace(NewCommentText));
    }

    public bool HasSelectedMovie => SelectedMovie is not null;

    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        private set
        {
            if (SetProperty(ref _selectedMovie, value))
            {
                OnPropertyChanged(nameof(HasSelectedMovie));
                OnPropertyChanged(nameof(SelectedMovieTitle));
                (AddCommentCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string SelectedMovieTitle => SelectedMovie?.Title ?? "No movie selected";

    public ObservableCollection<CommentNodeViewModel> RootComments { get; } = [];

    public string NewCommentText
    {
        get => _newCommentText;
        set
        {
            if (SetProperty(ref _newCommentText, value))
            {
                (AddCommentCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand AddCommentCommand { get; }

    private void OnSelectedMovieChanged(object? sender, Movie? movie)
    {
        SelectedMovie = movie;
        _ = LoadCommentsAsync();
    }

    private async Task LoadCommentsAsync()
    {
        RootComments.Clear();

        if (SelectedMovie is null)
        {
            StatusMessage = "Select a movie to view its discussion.";
            return;
        }

        try
        {
            var comments = await _commentService.GetCommentsForMovie(SelectedMovie.MovieId);
            var nodeLookup = comments.ToDictionary(
                comment => comment.MessageId,
                comment => new CommentNodeViewModel(comment, AddReplyAsync, DeleteCommentAsync));

            foreach (var comment in comments.OrderByDescending(comment => comment.CreatedAt))
            {
                var node = nodeLookup[comment.MessageId];
                if (comment.ParentCommentId.HasValue && nodeLookup.TryGetValue(comment.ParentCommentId.Value, out var parent))
                {
                    parent.Replies.Add(node);
                }
                else
                {
                    RootComments.Add(node);
                }
            }

            StatusMessage = comments.Count == 0 ? "No comments yet for this movie." : $"{comments.Count} comment(s) loaded.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task AddCommentAsync()
    {
        if (SelectedMovie is null)
        {
            return;
        }

        try
        {
            await _commentService.AddComment(LoggedInUserId, SelectedMovie.MovieId, NewCommentText);
            NewCommentText = string.Empty;
            await LoadCommentsAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task AddReplyAsync(int parentCommentId, string content)
    {
        try
        {
            await _commentService.AddReply(LoggedInUserId, parentCommentId, content);
            await LoadCommentsAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private async Task DeleteCommentAsync(int commentId)
    {
        try
        {
            await _commentService.DeleteComment(commentId);
            await LoadCommentsAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }
}
