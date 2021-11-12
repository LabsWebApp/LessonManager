namespace Models.Entities;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}