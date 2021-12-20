namespace ViewModelBase.Commands.ErrorHandlers;

public class WarningException : Exception
{
    public WarningException(string? message) : base(message) { }
}