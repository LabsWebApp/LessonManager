namespace ViewModelBase.Commands;

public interface IErrorHandler
{
    const string NoHandle = "NoHandle";
    void HandleError(Exception ex);
}