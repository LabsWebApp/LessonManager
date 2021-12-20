using Models.Entities.Proxies;

namespace Models.Repositories;

public interface IStudentsRep
{
    IQueryable<Student> Items { get; }
    IQueryable<ProxyEntity> ProxyItems => Items.Select(s => new ProxyStudent(s));
    void Add(Student student);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    void Delete(Guid id);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Student? Rename(Student student, string name);
    Task<Student?> RenameAsync(Student student, string name, CancellationToken cancellationToken = default);
    Student? GetStudentById(Guid id);
    Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void SetCourse(Student student, Course course);
    Task SetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default);

    void UnsetCourse(Student student, Course course);
    Task UnsetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default);

}