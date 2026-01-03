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

    // Provides weekly completed tasks report via stored procedure.
    public async Task<PagedResultDto<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(CompletedTasksPerWeekQueryDto query)
    {
        // 1. Get full data from repository (SP)
        var data = await _repository.GetCompletedTasksPerWeekAsync(
            query.StartDate,
            query.EndDate);

        // 2. Pagination logic
        var totalCount = data.Count;

        var items = data
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        // 3. Wrap result
        return new PagedResultDto<CompletedTasksPerWeekReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetTeamPerformanceSummaryAsync(request, cancellationToken);
    }
}

