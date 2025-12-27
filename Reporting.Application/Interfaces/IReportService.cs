using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IReportService
{
    Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQuery query);

    //Calls repository to get weekly completed tasks report
    Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQuery query);
}

