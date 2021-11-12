using Models.DataProviders;
using Models.DataProviders.SqLite;
using Models.DataProviders.SqLite.Repositories;
using Models.DataProviders.SqlServer;
using Models.DataProviders.SqlServer.Repositories;
using Models.Repositories;

namespace Models;

public record DataManager(IStudentsRep StudentsRep, ICoursesRep CoursesRep)
{
    public static DataManager Get(Provider provider)
    {
        switch (provider)
        {
            case Provider.SqLite:
                var liteContext = new SqLiteDbContext();
                return new(new SqLiteStudents(liteContext), new SqLiteCourses(liteContext));
            case Provider.SqlServer:
                var serverContext = new SqlSerDbContext();
                return new(new SqlServerStudents(serverContext), new SqlServerCourses(serverContext));
            default:
                throw new NotSupportedException(
                    $"Провайдер - {provider} не поддерживается в настоящее время.");
        }
    }
}