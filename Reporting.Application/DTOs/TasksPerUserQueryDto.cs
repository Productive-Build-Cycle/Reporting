namespace Reporting.Application.DTOs;

//public record TasksPerUserQueryDto(
//    string? Status,
//    DateTime? From,
//    DateTime? To,
//    bool UseRawSql = false
//);
public class TasksPerUserQueryDto
{
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? TeamId { get; set; }
    public bool UseSp { get; set; } = false;
    public bool UseRawSql { get; set; } = false;
    public bool UseDapper { get; set; } = false; // <- new
}
