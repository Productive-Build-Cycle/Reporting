using Reporting.Application.DTOs;

namespace Reporting.Application.Interfaces;

/// <summary>
/// Query interface for retrieving completed tasks per week report data.
/// This interface allows for multiple implementations (EF Core, Dapper, Stored Procedure, etc.)
/// following Clean Architecture principles.
/// 
/// Purpose: Returns a weekly breakdown of completed tasks within a specified date range.
/// </summary>
public interface ICompletedTasksPerWeekQuery
{
    /// <summary>
    /// Retrieves completed tasks per week report data based on the provided query parameters.
    /// </summary>
    /// <param name="startDate">Start date for the report period</param>
    /// <param name="endDate">End date for the report period</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of weekly completed task counts, ordered by year and week number</returns>
    Task<List<CompletedTasksPerWeekReportDto>> ExecuteAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

