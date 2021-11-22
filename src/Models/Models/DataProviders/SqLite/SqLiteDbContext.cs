namespace Models.DataProviders.SqLite;

public class SqLiteDbContext : EfDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder) => 
        optionBuilder.UseSqlite(@"Data Source = C:\Users\vovikdoc\source\repos\LessonManager\src\Models\Models\_DevData\SqLite\LessonManager.db");
}