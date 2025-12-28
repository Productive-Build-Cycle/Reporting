namespace Reporting.Application.DTOs;

public class TeamPerformanceSummaryRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TeamId { get; set; } // Optional: filter by specific team
}

