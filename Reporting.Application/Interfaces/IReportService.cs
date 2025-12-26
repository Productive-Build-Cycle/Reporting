using Reporting.Application.DTOs;
namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserRequestDto>> GetTasksPerUserAsync(
        bool useRawSql = false,
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);
}
