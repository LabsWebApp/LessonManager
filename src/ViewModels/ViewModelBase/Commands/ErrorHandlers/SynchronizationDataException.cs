namespace ViewModelBase.Commands.ErrorHandlers;

public class SynchronizationDataException : Exception
{
    public const string SyncErrMsg = "База данных была изменена извне, возможно, в другом приложении - перегрузите её.";

    public SynchronizationDataException(
        Action<object?>? onSyncException = null, object? parameter = null, string message = SyncErrMsg) 
        : base(message) => onSyncException?.Invoke(parameter ?? message);
}