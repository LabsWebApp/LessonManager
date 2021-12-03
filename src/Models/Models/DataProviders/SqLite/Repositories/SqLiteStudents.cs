namespace Models.DataProviders.SqLite.Repositories;

public class SqLiteStudents : EfStudents
{
    public SqLiteStudents(SqLiteDbContext context) : base(context) { }
}