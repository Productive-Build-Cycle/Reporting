namespace Reporting.Application.DTOs;
public class TasksPerUserReportDto
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public int TasksCount { get; set; }
    //public string? TeamName { get; set; }
}
