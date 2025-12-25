namespace Reporting.Domain.Entities;

public sealed class TaskEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = default!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public string Status { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
