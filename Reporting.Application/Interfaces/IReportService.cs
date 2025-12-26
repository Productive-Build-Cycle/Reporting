using Reporting.Application.DTOs;
namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        bool useRawSql = false,
        string? status = null,
        DateTime? from = null,
        DateTime? to = null);
}
