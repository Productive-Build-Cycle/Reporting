namespace Reporting.Application.DTOs;

public class TeamPerformanceSummaryRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TeamId { get; set; } // Optional: filter by specific team
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

