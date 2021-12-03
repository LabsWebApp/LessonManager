namespace Models.Entities;

public record ProxyEntity 
{
    public Guid Id { get;}
    public string Name { get; set; }
    public int Count { get; }

    public ProxyEntity(EntityBase entity)
    {
        Count = entity switch
        {
            Student student => student.Courses.Count,
            Course course => course.Students.Count,
            _ => throw new ArgumentException("Такой сущности нет в проекте.", nameof(entity))
        };
        Id = entity.Id;
        Name = entity.Name;
    }
}