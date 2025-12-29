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

    Task<List<TasksPerUserReportDto>> GetTasksPerUser_SpAsync(
    string? status = null,
    DateTime? from = null,
    DateTime? to = null,
    int? teamId = null);


    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
           CompletedTasksPerWeekQueryDto query);

    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);           
}
