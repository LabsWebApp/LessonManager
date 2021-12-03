﻿namespace Models.DataProviders.EfBase.Repositories;

public abstract class EfStudents : EfDisposable, IStudentsRep
{
    protected EfStudents(EfDbContext context) : base(context) { }

    public IQueryable<Student> Items => Context.Students.Include(s => s.Courses);

    public void Add(Student student)
    {
        if (student.Id == default)
        {
            Context.Add(student);
            Context.SaveChanges();
            return;
        }
        if (Context.Students.FirstOrDefault(s => s.Id == student.Id) is not null)
            return;
        Context.Add(student);
        Context.SaveChanges();
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default) => 
        Task.Run(async () =>
    {
        if (student.Id == default)
        {
            await Context.AddAsync(student, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
            return;
        }
        if (Context.Students.FirstOrDefault(s => s.Id == student.Id) is not null)
            return;
        await Context.AddAsync(student, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        Context.RejectChanges(cancellationToken);
    }, cancellationToken);

    public void Delete(Guid id)
    {
        var result = Context.Students.FirstOrDefault(s => s.Id == id);
        if (result == default) return;
        var courses = Context.Courses.Where(s => s.Students.Contains(result));
        foreach (var item in courses)
        {
            item.Students.Remove(result);
        }

        Context.Remove(result);
        Context.SaveChanges();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await Context
                .Students
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (result == default) return;
            var courses = Context
                .Courses
                .Where(s => s.Students.Contains(result));
            foreach (var item in courses)
            {
                if (!cancellationToken.IsCancellationRequested)
                    item.Students.Remove(result);
                else
                    break;
            }

            if (Context.RejectChanges(cancellationToken) > 0) return;

            Context.Remove(result);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public void Rename(Student student, string name)
    {
        var result = Context.Students.FirstOrDefault(s => s.Id == student.Id);
        if (result == null) throw new ArgumentException("Такого студента не существует");
        if (student.Name != name)
        {
            student.Name = name;
            Context.Update(student);
            Context.SaveChanges();
        }
    }
    public Task RenameAsync(Student student, string name, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var result = await Context.Students.FirstOrDefaultAsync(s => s.Id == student.Id,cancellationToken);
            if (result == null) return;
            if (student.Name != name)
            {
                student.Name = name;
                Context.Update(student);
                await Context.SaveChangesAsync(cancellationToken);
                Context.RejectChanges(cancellationToken);
            }
        }, cancellationToken);

    public Student? GetStudentById(Guid id) =>
        Context.Students.FirstOrDefault(s => s.Id == id);

    public Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Students.FirstOrDefaultAsync(s => s.Id == id,cancellationToken);

    public void SetCourse(Student student, Course course)
    {
        if (!Context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
        if (student.Courses.Contains(course)) return;
        student.Courses.Add(course);
        Context.SaveChanges();
    }

    public Task SetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            if (!Context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
            if (student.Courses.Contains(course)) return;
            student.Courses.Add(course);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
        }, cancellationToken);

    public void UnsetCourse(Student student, Course course)
    {
        if (!Context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
        if (!student.Courses.Contains(course)) return;
        student.Courses.Remove(course);
        Context.SaveChanges();
    }

    public Task UnsetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            if (!Context.Courses.Contains(course)) throw new ArgumentException("Курс не создан");
            if (!student.Courses.Contains(course)) return;
            student.Courses.Remove(course);
            await Context.SaveChangesAsync(cancellationToken);
            Context.RejectChanges(cancellationToken);
        }, cancellationToken);
}