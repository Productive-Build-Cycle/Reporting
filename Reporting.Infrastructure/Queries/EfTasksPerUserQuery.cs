using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Entity Framework Core implementation of ITasksPerUserQuery using LINQ.
/// This implementation uses EF Core's LINQ-to-SQL translation for querying tasks per user data.
/// 
/// Performance characteristics:
/// - EF Core translates LINQ to SQL, which is readable but may generate suboptimal queries
/// - Good for development and when query structure changes frequently
/// - Overhead from LINQ expression tree compilation and SQL generation
/// </summary>
public sealed class EfTasksPerUserQuery : ITasksPerUserQuery
{
    private readonly ReportingDbContext _context;

    public EfTasksPerUserQuery(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<TasksPerUserReportDto>> ExecuteAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        // Start with all tasks, using AsNoTracking for read-only queries (better performance)
        var tasksQuery = _context.Tasks
            .AsNoTracking()
            .Include(t => t.User) // Include User for Name property
            .AsQueryable();

        // Apply optional status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            tasksQuery = tasksQuery.Where(t => t.Status == status);
        }

        // Apply optional date range filters
        if (from.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt <= to.Value);
        }

        // Group by user and count tasks, then order by count descending
        var result = await tasksQuery
            .GroupBy(t => new { t.UserId, t.User.Name })
            .Select(g => new TasksPerUserReportDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                TasksCount = g.Count()
            })
            .OrderByDescending(x => x.TasksCount)
            .ToListAsync(cancellationToken);

        return result;
    }
}

