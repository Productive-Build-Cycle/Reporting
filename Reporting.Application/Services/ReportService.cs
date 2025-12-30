using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Application.Services;

/// <summary>
/// Service for generating various reports.
/// This service acts as a coordinator, delegating to specialized query implementations
/// following Clean Architecture principles. The service layer doesn't contain data access logic,
/// but instead uses query interfaces that can have multiple implementations (EF, Dapper, SP).
/// 
/// Benefits:
/// - Separation of concerns: business logic separate from data access
/// - Testability: can mock query interfaces
/// - Flexibility: can swap implementations without changing service code
/// </summary>
public class ReportService : IReportService
{
    private readonly ITasksPerUserQuery _tasksPerUserQuery;
    private readonly ICompletedTasksPerWeekQuery _completedTasksPerWeekQuery;
    private readonly ITeamPerformanceQuery _teamPerformanceQuery;

    /// <summary>
    /// Initializes a new instance of ReportService with required query dependencies.
    /// </summary>
    /// <param name="tasksPerUserQuery">Query implementation for tasks per user report</param>
    /// <param name="completedTasksPerWeekQuery">Query implementation for completed tasks per week report</param>
    /// <param name="teamPerformanceQuery">Query implementation for team performance summary</param>
    public ReportService(
        ITasksPerUserQuery tasksPerUserQuery,
        ICompletedTasksPerWeekQuery completedTasksPerWeekQuery,
        ITeamPerformanceQuery teamPerformanceQuery)
    {
        _tasksPerUserQuery = tasksPerUserQuery;
        _completedTasksPerWeekQuery = completedTasksPerWeekQuery;
        _teamPerformanceQuery = teamPerformanceQuery;
    }

    /// <summary>
    /// Gets tasks per user report with optional filters.
    /// Validates input parameters before delegating to query implementation.
    /// </summary>
    /// <param name="query">Query parameters including status and date range filters</param>
    /// <returns>List of users with their task counts</returns>
    /// <exception cref="ArgumentException">Thrown when date range is invalid</exception>
    public async Task<List<TasksPerUserReportDto>> GetTasksPerUserAsync(
        TasksPerUserQueryDto query)
    {
        // Validate date range
        if (query.From.HasValue && query.To.HasValue &&
            query.From > query.To)
        {
            throw new ArgumentException("From date cannot be greater than To date");
        }

        // Delegate to query implementation (EF, Dapper, or SP based on DI configuration)
        return await _tasksPerUserQuery.ExecuteAsync(
            query.Status,
            query.From,
            query.To);
    }

    /// <summary>
    /// Gets weekly breakdown of completed tasks within a date range.
    /// Delegates to query implementation for data retrieval.
    /// </summary>
    /// <param name="query">Query parameters with start and end dates</param>
    /// <returns>List of weekly completed task counts</returns>
    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
        CompletedTasksPerWeekQueryDto query)
    {
        // Delegate to query implementation (EF, Dapper, or SP based on DI configuration)
        return await _completedTasksPerWeekQuery.ExecuteAsync(
            query.StartDate,
            query.EndDate);
    }

    /// <summary>
    /// Gets team performance summary with metrics like total tasks, completed tasks, and completion rate.
    /// Delegates to query implementation for data retrieval.
    /// </summary>
    /// <param name="request">Query parameters including date range and optional team filter</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of team performance summaries</returns>
    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Delegate to query implementation (EF, Dapper, or SP based on DI configuration)
        return await _teamPerformanceQuery.ExecuteAsync(request, cancellationToken);
    }
}

