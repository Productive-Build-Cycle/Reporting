using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportRepository
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserDapperAsync(
        TasksPerUserQueryDto query,
        CancellationToken cancellationToken);
   
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
           CompletedTasksPerWeekQueryDto query);

    Task<PagedResultDto<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);

}
