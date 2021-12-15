namespace Models.Entities.Proxies;

public record ProxyCourse : ProxyEntity
{
    public ProxyCourse(Course entity) : base(entity) =>
        Count = entity.Students.Count;
}