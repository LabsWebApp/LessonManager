using Models.DataProviders.SqLite;
using Models.DataProviders.SqLite.Repositories;
using Models.DataProviders.SqlServer;
using Models.DataProviders.SqlServer.Repositories;

namespace Models;

public record DataManager(IStudentsRep StudentsRep, ICoursesRep CoursesRep, Provider Provider) 
    : IAsyncDisposable, IDisposable
{
    internal DbContext? Context { private get; init; }

    public static DataManager Get(Provider provider)
    {
        switch (provider)
        {
            case Provider.SqLite:
                var liteContext = new SqLiteDbContext();
                return new(new SqLiteStudents(liteContext),
                    new SqLiteCourses(liteContext), Provider.SqLite) 
                    { Context = liteContext };
            case Provider.SqlServer:
                var serverContext = new SqlSerDbContext();
                return new(new SqlServerStudents(serverContext), 
                    new SqlServerCourses(serverContext), Provider.SqlServer)
                    { Context = serverContext };
            default:
                throw new NotSupportedException(
                    $"Провайдер - {provider} не поддерживается в настоящее время.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context?.Dispose();
        }
    }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (Context is not null)
        {
            await Context.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}

