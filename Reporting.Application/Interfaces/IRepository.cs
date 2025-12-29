using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportRepository
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUser_LinqAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<List<TasksPerUserReportDto>> GetTasksPerUser_RawSqlAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
           CompletedTasksPerWeekQueryDto query);

    // Gets weekly completed tasks from the GetCompletedTasksPerWeek stored procedure.
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekSpAsync(
        DateTime startDate,
        DateTime endDate);

    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);           
}
