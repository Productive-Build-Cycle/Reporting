using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

/// <summary>
/// Query interface for retrieving team performance summary data.
/// This interface allows for multiple implementations (EF Core, Dapper, Stored Procedure, etc.)
/// following Clean Architecture principles.
/// </summary>
public interface ITeamPerformanceQuery
{
    /// <summary>
    /// Retrieves team performance summary data based on the provided request parameters.
    /// </summary>
    /// <param name="request">Filter parameters including date range and optional team filter</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of team performance summaries with metrics</returns>
    Task<List<TeamPerformanceSummaryResponseDto>> ExecuteAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default);
}

