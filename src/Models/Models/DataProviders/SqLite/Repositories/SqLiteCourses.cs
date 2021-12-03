namespace Models.DataProviders.SqLite.Repositories;

public class SqLiteCourses : EfCourses
{
    public SqLiteCourses(SqLiteDbContext context) : base(context) { }
}