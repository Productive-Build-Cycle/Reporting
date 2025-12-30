using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Entity Framework Core implementation of ITeamPerformanceQuery using LINQ.
/// This implementation uses EF Core's LINQ-to-SQL translation for querying team performance data.
/// 
/// Performance characteristics:
/// - EF Core translates LINQ to SQL, which is readable but may generate suboptimal queries
/// - Good for development and when query structure changes frequently
/// - Overhead from LINQ expression tree compilation and SQL generation
/// - AsQueryable() allows deferred execution until ToListAsync()
/// </summary>
public sealed class EfTeamPerformanceQuery : ITeamPerformanceQuery
{
    private readonly ReportingDbContext _context;

    public EfTeamPerformanceQuery(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> ExecuteAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Start with all tasks as IQueryable for deferred execution
        // This allows EF Core to build the SQL query incrementally
        var tasksQuery = _context.Tasks.AsQueryable();

        // Apply optional date filters if provided
        // These filters are translated to SQL WHERE clauses by EF Core
        if (request.StartDate.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        // Apply optional team filter if provided
        if (request.TeamId.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.TeamId == request.TeamId.Value);
        }

        // Join with Teams table and group by team to calculate metrics
        // This query calculates:
        // - TotalTasks: Count of all tasks per team
        // - CompletedTasks: Count of tasks with Status='Completed' OR CompletedAt IS NOT NULL
        // - CompletionRate: Percentage of completed tasks (0-100)
        var result = await (from task in tasksQuery
                           join team in _context.Teams on task.TeamId equals team.Id
                           group task by new { team.Id, team.Name } into g
                           select new TeamPerformanceSummaryResponseDto
                           {
                               TeamId = g.Key.Id,
                               TeamName = g.Key.Name,
                               TotalTasks = g.Count(),
                               // Count tasks that are completed (either by status or completion date)
                               CompletedTasks = g.Count(t => t.Status == "Completed" || t.CompletedAt != null),
                               // Calculate completion rate as percentage (avoid division by zero)
                               CompletionRate = g.Count() > 0
                                   ? (decimal)g.Count(t => t.Status == "Completed" || t.CompletedAt != null) * 100 / g.Count()
                                   : 0
                           })
            .ToListAsync(cancellationToken);

        return result;
    }
}

