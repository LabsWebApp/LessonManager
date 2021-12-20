using Models.Entities.Proxies;

namespace Models.Repositories;

public interface ICoursesRep
{
    IQueryable<Course> Items { get; }
    IQueryable<ProxyCourse> ProxyItems => Items.Select(c => new ProxyCourse(c));
    void Add(Course course);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    void Delete(Guid id);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Course? GetCourseById(Guid id);
    Task<Course?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);
}