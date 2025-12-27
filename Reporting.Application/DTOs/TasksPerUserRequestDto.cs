namespace Reporting.Application.DTOs;
public class TasksPerUserRequestDto 
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int TasksCount { get; set; }
}
