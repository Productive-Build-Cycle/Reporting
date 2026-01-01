using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportRepository
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUser_DapperAsync(
    string? status = null,
    DateTime? from = null,
    DateTime? to = null);


    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
           CompletedTasksPerWeekQueryDto query);

    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);           
}
