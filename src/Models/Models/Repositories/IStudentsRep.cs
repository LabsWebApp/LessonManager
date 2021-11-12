namespace Models.Repositories;

public interface IStudentsRep
{
    IQueryable<Student> Items { get; }
    void Add(Student student);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    void Delete(Guid id);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void Rename(Student student, string name);
    Task RenameAsync(Student student, string name, CancellationToken cancellationToken = default);
    Student? GetStudentById(Guid id);
    Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void SetCourse(Student student, Course course);
    Task SetCourseAsync(Student student, Course course, CancellationToken cancellationToken = default);
        
}