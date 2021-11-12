namespace Models.Entities;

public class Student : EntityBase
{
    public virtual IList<Course> Courses { get; set; } = new List<Course>();
}