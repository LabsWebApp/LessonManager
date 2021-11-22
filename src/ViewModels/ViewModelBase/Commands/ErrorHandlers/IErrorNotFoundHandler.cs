namespace ViewModelBase.Commands.ErrorHandlers;

public interface IErrorNotFoundHandler : IErrorHandler
{
    void HandleResultNotFound(ResultNotFoundException ex);
}