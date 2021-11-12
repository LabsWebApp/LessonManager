namespace ViewModelBase.Commands;

public interface IErrorCancelHandler : IErrorHandler
{
    void HandleCancel(OperationCanceledException ex);
}