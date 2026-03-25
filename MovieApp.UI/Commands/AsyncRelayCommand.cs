#nullable enable

using System.Windows.Input;

namespace MovieApp.UI.Commands;

/// <summary>
/// Provides an asynchronous relay command implementation.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Predicate<object?>? _canExecute;
    private bool _isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The asynchronous execute callback.</param>
    /// <param name="canExecute">The can execute callback.</param>
    public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Notifies the UI that the command execution state has changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
