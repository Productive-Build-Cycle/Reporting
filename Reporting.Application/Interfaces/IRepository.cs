using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportRepository
{

  // Gets weekly completed tasks from the GetCompletedTasksPerWeek stored procedure.
    Task<List<TasksPerUserReportDto>> GetTasksPerUserEfAsync(
        TasksPerUserQueryDto query,
        CancellationToken cancellationToken = default);
   
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
        DateTime startDate,
        DateTime endDate);

    Task<PagedResultDto<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);

}
