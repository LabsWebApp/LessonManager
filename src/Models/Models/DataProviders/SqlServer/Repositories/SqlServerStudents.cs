using Models.Repositories;

namespace Models.DataProviders.SqlServer.Repositories;

public class SqlServerStudents : IStudentsRep
{
    private readonly SqlSerDbContext _context;

    public SqlServerStudents(SqlSerDbContext context) => _context = context;

    public IQueryable<Student> Items => _context.Students.Include(s => s.Courses);

    public void Add(Student student)
    {
        if (_context.Students.FirstOrDefault(c => c.Name.ToLower() == student.Name.ToLower()) != default)
            throw new ArgumentException("Такой студен уже существует.");
        if (student.Id == default)
        {
            _context.Add(student);
            _context.SaveChanges();
            return;
        }
        if (_context.Students.FirstOrDefault(s => s.Id == student.Id) is not null)
            throw new ArgumentException();
        _context.Add(student);
        _context.SaveChanges();
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default) => 
        Task.Run(async () =>
    {
        if (await _context.Students.FirstOrDefaultAsync(c => c.Name.ToLower() == student.Name.ToLower(), cancellationToken: cancellationToken) != default)
            throw new ArgumentException("Такой студен уже существует.");
        if (student.Id == default)
        {
            await _context.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.RejectChanges(cancellationToken);
            return;
        }
        if (_context.Students.FirstOrDefault(s => s.Id == student.Id) is not null)
            throw new ArgumentException();
        await _context.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _context.RejectChanges(cancellationToken);
    }, cancellationToken);

    public void Delete(Guid id)
    {
        var result = _context.Students.FirstOrDefault(s => s.Id == id);
        if (result == default) return;
        var courses = _context.Courses.Where(s => s.Students.Contains(result));
        foreach (var item in courses)
        {
            item.Students.Remove(result);
        }

        _context.Remove(result);
        _context.SaveChanges();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await _context
                .Students
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (result == default) return;
            var courses = _context
                .Courses
                .Where(s => s.Students.Contains(result));
            foreach (var item in courses)
            {
                if (!cancellationToken.IsCancellationRequested)
                    item.Students.Remove(result);
                else
                    break;
            }

            if (_context.RejectChanges(cancellationToken) > 0) return;

            _context.Remove(result);
            await _context.SaveChangesAsync(cancellationToken);
            _context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public void Rename(Student student, string name)
    {
        var result = _context.Students.FirstOrDefault(s => s.Id == student.Id);
        if (result == null) throw new ArgumentException("Такого студента не существует");
        if (student.Name != name)
        {
            student.Name = name;
            _context.Update(student);
            _context.SaveChanges();
        }
    }
    public Task RenameAsync(Student student, string name, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await _context.Students.FirstOrDefaultAsync(s => s.Id == student.Id,cancellationToken);
            if (result == null) return;
            if (student.Name != name)
            {
                student.Name = name;
                _context.Update(student);
                await _context.SaveChangesAsync(cancellationToken);
                _context.RejectChanges(cancellationToken);
            }
        }, cancellationToken);

    public Student? GetStudentById(Guid id) =>
        _context.Students.FirstOrDefault(s => s.Id == id);

    public Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Students.FirstOrDefaultAsync(s => s.Id == id,cancellationToken);

    public void SetCourse(Student student, Course course)
    {
        if (!_context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
        if (student.Courses.Contains(course)) return;
        student.Courses.Add(course);
        _context.SaveChanges();
    }

    public Task SetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            if (!_context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
            if (student.Courses.Contains(course)) return;
            student.Courses.Add(course);
            await _context.SaveChangesAsync(cancellationToken);
            _context.RejectChanges(cancellationToken);
        }, cancellationToken);
}