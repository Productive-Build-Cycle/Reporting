using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using System.Threading.Tasks;

namespace Reporting.Application.Services;

public class ReportService : IReportService
{
    private readonly ITasksPerUserReportRepository _repository;

    public ReportService(ITasksPerUserReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQuery query)
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
            CompletedTasksPerWeekQuery query)
    {
        return await _repository
            .GetCompletedTasksPerWeekAsync(query);
    }
}

