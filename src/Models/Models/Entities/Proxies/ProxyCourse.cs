namespace Models.Entities.Proxies;

public record ProxyCourse : ProxyEntity
{
    public bool IsSelected { get; set; }
    public ProxyCourse(Course entity) : base(entity) =>
        Count = entity.Students.Count;
}