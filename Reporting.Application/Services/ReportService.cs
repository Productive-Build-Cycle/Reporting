using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Threading.Tasks;

namespace Reporting.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQueryDto query)
    {
        if (query.From.HasValue && query.To.HasValue &&
            query.From > query.To)
        {
            throw new ArgumentException("From date cannot be greater than To date");
        }

        return query.UseRawSql
            ? await _repository.GetTasksPerUser_RawSqlAsync(
                query.Status, query.From, query.To)
            : await _repository.GetTasksPerUser_LinqAsync(
                query.Status, query.From, query.To);
    }

    //Calls repository to get weekly completed tasks report
    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQueryDto query)
    {
        return await _repository
            .GetCompletedTasksPerWeekAsync(query);
    }

    // Provides weekly completed tasks report via stored procedure.
    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekSpAsync(CompletedTasksPerWeekQueryDto query)
    {
        return await _repository
            .GetCompletedTasksPerWeekSpAsync(
                query.StartDate,
                query.EndDate);
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetTeamPerformanceSummaryAsync(request, cancellationToken);
    }
}

