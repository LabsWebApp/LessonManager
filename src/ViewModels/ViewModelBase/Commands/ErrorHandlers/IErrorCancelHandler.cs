namespace ViewModelBase.Commands.ErrorHandlers;

public interface IErrorCancelHandler : IErrorHandler
{
    void HandleCancel(OperationCanceledException ex);
}