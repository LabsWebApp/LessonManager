namespace Models.Entities.Proxies;

public record ProxyStudent : ProxyEntity
{
    public ProxyStudent(Student entity) : base(entity) =>
        Count = entity.Courses.Count;
}