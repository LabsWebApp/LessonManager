namespace Models.DataProviders.SqlServer.Repositories;

public class SqlServerStudents : EfStudents
{
    public SqlServerStudents(SqlSerDbContext context) : base(context) { }
}