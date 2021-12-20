namespace Models.DataProviders.EfBase;

public abstract class EfDbContext : DbContext
{
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Student>().HasKey(s => s.Id);
        mb.Entity<Course>().HasKey(c => c.Id);
    }
}