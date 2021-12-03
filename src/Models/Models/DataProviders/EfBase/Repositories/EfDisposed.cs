namespace Models.DataProviders.EfBase.Repositories;

public abstract class EfDisposable : IDisposable
{
    protected readonly EfDbContext Context;
    private bool _disposedValue;

    protected EfDisposable(EfDbContext context) => Context = context;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: освободить управляемое состояние (управляемые объекты)
                Context.Dispose();
            }

            // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
            // TODO: установить значение NULL для больших полей
            _disposedValue = true;
        }
    }

    // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
    // ~EfDisposed()
    // {
    //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}