namespace ViewModelBase.Commands.ErrorHandlers;

public class SynchronizationDataException : Exception
{
    public const string SyncErrMsg = "Данные были изменены извне, возможно, другим клиентом - перегрузите данные.";

    public SynchronizationDataException(
        Action<object?>? onSyncException = null, object? parameter = null, string message = SyncErrMsg) 
        : base(message) => onSyncException?.Invoke(parameter ?? message);
}