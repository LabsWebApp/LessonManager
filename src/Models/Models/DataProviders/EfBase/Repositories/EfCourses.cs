namespace Models.DataProviders.EfBase.Repositories;

public abstract class EfCourses : EfDisposable, ICoursesRep
{
    protected EfCourses(EfDbContext context) : base(context) {}

    public IQueryable<Course> Items => Context.Courses.Include(c => c.Students);

    public void Add(Course course)
    {
        if(course.Id == default)
        {
            Context.Add(course);
            Context.SaveChanges();
            return;
        }
        var result = Context.Courses.FirstOrDefault(s => s.Id == course.Id);
        if (result is not null) throw new ArgumentException("");
        Context.Add(course);
        Context.SaveChanges();
    }
    public Task AddAsync(Course course, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            if (course.Id == default)
            {
                await Context.AddAsync(course, cancellationToken);
                await Context.SaveChangesAsync(cancellationToken);
                Context.RejectChanges(cancellationToken);
                return;
            }
            if (Context.Students.FirstOrDefault(s => s.Id == course.Id) is not null)
                throw new ArgumentException("");
            await Context.AddAsync(course, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public void Delete(Guid id)
    {
        var result = Context.Courses.FirstOrDefault(s => s.Id == id);
        if (result == default) return;
        var students = Context.Students.Where(s => s.Courses.Contains(result));
        foreach (var item in students)
        {
            item.Courses.Remove(result);
        }
        Context.Remove(result);
        Context.SaveChanges();
    }
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await Context
                .Courses
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (result == default) return;
            var students = Context
                .Students
                .Where(s => s.Courses.Contains(result));
            foreach (var item in students)
            {
                if (!cancellationToken.IsCancellationRequested)
                    item.Courses.Remove(result);
                else
                    break;
            }

            if (Context.RejectChanges(cancellationToken) > 0) return;

            Context.Remove(result);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public Course? GetCourseById(Guid id) =>
        Context.Courses.FirstOrDefault(s => s.Id == id);
    public Task<Course?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Courses.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}