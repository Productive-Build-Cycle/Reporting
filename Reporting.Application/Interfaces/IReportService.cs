using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQueryDto query);

    //Calls repository to get weekly completed tasks report
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQueryDto query);

    // Provides weekly completed tasks report via stored procedure.
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekSpAsync(
        CompletedTasksPerWeekQueryDto query);

    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);            
}

