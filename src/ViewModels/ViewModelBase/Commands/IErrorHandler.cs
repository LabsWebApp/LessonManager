namespace ViewModelBase.Commands;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}