namespace ViewModelBase.Commands.ErrorHandlers;

public interface IWarningHandler : IErrorHandler
{
    void WarningHandle(WarningException ex);
}