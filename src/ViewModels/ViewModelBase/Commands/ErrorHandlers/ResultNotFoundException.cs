namespace ViewModelBase.Commands.ErrorHandlers;

public class ResultNotFoundException : Exception
{
    public ResultNotFoundException(string? message) : base(message)
    {
    }
}