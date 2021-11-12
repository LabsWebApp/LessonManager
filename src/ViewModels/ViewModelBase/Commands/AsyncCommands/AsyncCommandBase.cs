namespace ViewModelBase.Commands.AsyncCommands;

public abstract class AsyncCommandBase
{
    public event EventHandler? CanExecuteChanged;

    protected bool IsExecuting;
    protected readonly IErrorHandler? ErrorCancelHandler;
    protected CancellationTokenSource? CancellationSource;

    protected AsyncCommandBase(IErrorHandler? errorCancelHandler,
        CancellationTokenSource? cancel, bool cancellationSupport)
    {
        if (cancellationSupport)
            CancellationSource = cancel ?? new CancellationTokenSource();
        ErrorCancelHandler = errorCancelHandler;
    }

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public bool IsNotCancelled =>
        !CancellationSource?.IsCancellationRequested ??
        throw new NotSupportedException("Данная команда не поддерживает отмену.");

    private void CancelException()
    {
        if (CancellationSource is null)
            throw new NotSupportedException("Данная команда не поддерживает отмену.");
    }

    public void Cancel()
    {
        CancelException();
        if (CancellationSource is { IsCancellationRequested: false })
        {
            CancellationSource?.Cancel();
        }
    }

    public void ResetCancel(CancellationTokenSource? cancellationSource = null)
    {
        CancelException();
        if (CancellationSource is { IsCancellationRequested: true })
            cancellationSource = new();
        CancellationSource = cancellationSource ?? new CancellationTokenSource();
    }
}