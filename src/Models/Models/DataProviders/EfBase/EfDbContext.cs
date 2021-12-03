namespace Models.DataProviders.EfBase;

public abstract class EfDbContext : DbContext
{
    protected EfDbContext() => Database.EnsureCreated();
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Student>().HasKey(s => s.Id);
        mb.Entity<Course>().HasKey(c => c.Id);
    }
}