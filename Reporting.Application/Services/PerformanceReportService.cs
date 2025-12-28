using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Application.Services;

public class PerformanceReportService : IPerformanceReportService
{
    private readonly IPerformanceRepository _repository;

    public PerformanceReportService(IPerformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetTeamPerformanceSummaryAsync(request, cancellationToken);
    }
}

