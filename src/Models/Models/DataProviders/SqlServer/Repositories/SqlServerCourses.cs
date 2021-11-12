using Models.Repositories;

namespace Models.DataProviders.SqlServer.Repositories;

public class SqlServerCourses : ICoursesRep
{
    private readonly SqlSerDbContext _context;

    public SqlServerCourses(SqlSerDbContext context) => this._context = context;

    public IQueryable<Course> Items => _context.Courses.Include(c => c.Students);

    public void Add(Course course)
    {
        if(course.Id == default)
        {
            _context.Add(course);
            _context.SaveChanges();
            return;
        }
        var result = _context.Courses.FirstOrDefault(s => s.Id == course.Id);
        if (result is not null) throw new ArgumentException("");
        _context.Add(course);
        _context.SaveChanges();
    }
    public Task AddAsync(Course course, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            if (course.Id == default)
            {
                await _context.AddAsync(course, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _context.RejectChanges(cancellationToken);
                return;
            }
            if (_context.Students.FirstOrDefault(s => s.Id == course.Id) is not null)
                throw new ArgumentException();
            await _context.AddAsync(course, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public void Delete(Guid id)
    {
        var result = _context.Courses.FirstOrDefault(s => s.Id == id);
        if (result == default) return;
        var students = _context.Students.Where(s => s.Courses.Contains(result));
        foreach (var item in students)
        {
            item.Courses.Remove(result);
        }
        _context.Remove(result);
        _context.SaveChanges();
    }
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await _context
                .Courses
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (result == default) return;
            var students = _context
                .Students
                .Where(s => s.Courses.Contains(result));
            foreach (var item in students)
            {
                if (!cancellationToken.IsCancellationRequested)
                    item.Courses.Remove(result);
                else
                    break;
            }

            if (_context.RejectChanges(cancellationToken) > 0) return;

            _context.Remove(result);
            await _context.SaveChangesAsync(cancellationToken);
            _context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public Course? GetCourseById(Guid id) =>
        _context.Courses.FirstOrDefault(s => s.Id == id);
    public Task<Course?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Courses.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}