using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

public interface IPerformanceReportService
{
    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);
}
