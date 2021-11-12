using System.Windows.Input;

namespace ViewModelBase.Commands.AsyncCommands;

public class AsyncCommand : AsyncCommandBase, IAsyncCommand
{
    private readonly Func<CancellationToken?, Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncCommand(
        Func<CancellationToken?, Task> execute,
        CancellationTokenSource? cancel = null,
        Func<bool>? canExecute = null,
        IErrorHandler? errorCancelHandler = null)
            : base(errorCancelHandler, cancel, true)
    {
        _execute = _ => execute(CancellationSource?.Token);
        _canExecute = canExecute;
    }

    public AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        IErrorHandler? errorHandler = null)
        : base(errorHandler, null, false)
    {
        _execute = _ => execute.Invoke();
        _canExecute = canExecute;
    }

    public bool CanExecute() =>
        !IsExecuting &&
        (_canExecute?.Invoke() ?? true) &&
        (!CancellationSource?.IsCancellationRequested ?? true);

    public async Task ExecuteAsync()
    {
        if (CanExecute())
        {
            try
            {
                IsExecuting = true;
                await _execute.Invoke(CancellationSource?.Token);
            }
            finally
            {
                IsExecuting = false;
            }
        }
        RaiseCanExecuteChanged();
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object? parameter) => CanExecute();

    void ICommand.Execute(object? parameter) =>
        ExecuteAsync().FireAndForgetSafeAsync(ErrorCancelHandler);
    #endregion
}