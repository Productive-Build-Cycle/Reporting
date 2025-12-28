namespace Reporting.Application.DTOs;

public class TeamPerformanceSummaryResponseDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = default!;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal CompletionRate { get; set; } // Percentage (0-100)
}

