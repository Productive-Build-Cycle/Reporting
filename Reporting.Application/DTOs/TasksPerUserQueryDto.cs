namespace Reporting.Application.DTOs;

public class TasksPerUserQueryDto
{
    public string? Status { get; set; } 
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

