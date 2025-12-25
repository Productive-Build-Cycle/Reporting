namespace Reporting.Domain.Entities;

public sealed class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
