using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQueryDto query,
        CancellationToken cancellationToken = default);

    //Calls repository to get weekly completed tasks report
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQueryDto query);

    Task<PagedResultDto<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);            
}

