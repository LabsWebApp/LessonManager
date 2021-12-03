namespace Models.DataProviders.SqlServer.Repositories;

public class SqlServerCourses : EfCourses
{
    public SqlServerCourses(SqlSerDbContext context) : base(context) { }
}