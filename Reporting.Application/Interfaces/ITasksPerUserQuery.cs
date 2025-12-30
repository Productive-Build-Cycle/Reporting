using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

/// <summary>
/// Query interface for retrieving tasks per user report data.
/// This interface allows for multiple implementations (EF Core, Dapper, Stored Procedure, etc.)
/// following Clean Architecture principles.
/// 
/// Purpose: Returns the count of tasks assigned to each user, optionally filtered by status and date range.
/// </summary>
public interface ITasksPerUserQuery
{
    /// <summary>
    /// Retrieves tasks per user report data based on the provided query parameters.
    /// </summary>
    /// <param name="status">Optional status filter (e.g., "Completed", "Pending")</param>
    /// <param name="from">Optional start date filter</param>
    /// <param name="to">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of users with their task counts, ordered by task count descending</returns>
    Task<List<TasksPerUserReportDto>> ExecuteAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}

