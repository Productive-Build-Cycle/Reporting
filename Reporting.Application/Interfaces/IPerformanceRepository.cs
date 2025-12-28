using Reporting.Application.DTOs;
using Reporting.Domain.Entities;

namespace Reporting.Application.Interfaces;

public interface IPerformanceRepository
{
    Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);
}
