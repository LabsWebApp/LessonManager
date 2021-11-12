global using Microsoft.EntityFrameworkCore;

namespace Models.DataProviders.SqlServer;

public class SqlSerDbContext : EfDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder) => 
        optionBuilder.UseSqlServer(
            @"Data Source=(local)\SQLEXPRESS; Database=LessonManager; Persist Security Info=false; User ID='sa'; Password='sa'; MultipleActiveResultSets=True; Trusted_Connection=False;");
}