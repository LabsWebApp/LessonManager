namespace Models.Entities.Proxies;

public abstract record ProxyEntity 
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int Count { get; init; }

    protected ProxyEntity(EntityBase entity)
    {
        Id = entity.Id;
        Name = entity.Name;
    }
}