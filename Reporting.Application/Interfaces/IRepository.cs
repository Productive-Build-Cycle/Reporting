using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface ITasksPerUserReportRepository
{
    Task<List<TasksPerUserRequestDto>> GetTasksPerUser_LinqAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<List<TasksPerUserRequestDto>> GetTasksPerUser_RawSqlAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);
}
