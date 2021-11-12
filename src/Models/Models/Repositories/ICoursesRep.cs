namespace Models.Repositories;

public interface ICoursesRep
{
    IQueryable<Course> Items { get; }
    void Add(Course course);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    void Delete(Guid id);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    //void Rename(Course course, string name);
    //Task RenameAsync(Course course, string name, CancellationToken cancellationToken = default);
    Course? GetCourseById(Guid id);
    Task<Course?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);
        

}