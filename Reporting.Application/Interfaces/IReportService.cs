using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQueryDto query,
        CancellationToken cancellationToken = default);

    // Provides weekly completed tasks report via stored procedure.
    Task<PagedResultDto<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
        CompletedTasksPerWeekQueryDto query);

    Task<PagedResultDto<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);            
}

