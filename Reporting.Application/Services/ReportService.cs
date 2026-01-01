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
            TasksPerUserQueryDto query,
            CancellationToken cancellationToken = default)
    {
        return await _repository.GetTasksPerUserDapperAsync(query, cancellationToken);
    }



    //Calls repository to get weekly completed tasks report
    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQueryDto query)
    {
        return await _repository
            .GetCompletedTasksPerWeekAsync(query);
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetTeamPerformanceSummaryAsync(request, cancellationToken);
    }
}

