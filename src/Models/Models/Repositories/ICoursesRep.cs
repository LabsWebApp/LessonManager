namespace Models.Repositories;

public interface ICoursesRep
{
    IQueryable<Course> Items { get; }
    IQueryable<ProxyEntity> ProxyItems => Items.Select(c => new ProxyEntity(c));
    void Add(Course course);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    void Delete(Guid id);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Course? GetCourseById(Guid id);
    Course? GetCourseByProxy(ProxyEntity proxy) => GetCourseById(proxy.Id);
    Task<Course?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);
        

}