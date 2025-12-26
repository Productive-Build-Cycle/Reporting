using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Application.Services;

public class ReportService : IReportService
{
    private readonly ITasksPerUserReportRepository _repository;

    public ReportService(ITasksPerUserReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TasksPerUserRequestDto>> GetTasksPerUserAsync(
        bool useRawSql = false,
        string? status = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        if (useRawSql)
            return await _repository.GetTasksPerUser_RawSqlAsync(status, from, to);

        return await _repository.GetTasksPerUser_LinqAsync(status, from, to);
    }
}
