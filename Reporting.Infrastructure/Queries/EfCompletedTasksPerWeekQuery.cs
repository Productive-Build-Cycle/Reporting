using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Queries;

/// <summary>
/// Entity Framework Core implementation of ICompletedTasksPerWeekQuery using LINQ.
/// This implementation uses EF Core's LINQ-to-SQL translation for querying weekly completed tasks.
/// 
/// Performance characteristics:
/// - EF Core translates LINQ to SQL, handling date/week calculations
/// - Readable code but may generate complex SQL for date functions
/// - Good for development when business logic changes frequently
/// </summary>
public sealed class EfCompletedTasksPerWeekQuery : ICompletedTasksPerWeekQuery
{
    private readonly ReportingDbContext _context;

    public EfCompletedTasksPerWeekQuery(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompletedTasksPerWeekReportDto>> ExecuteAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Query completed tasks within the date range
        // Note: EF Core doesn't have direct support for DATEPART(WEEK), so we use FromSqlRaw
        // This is a limitation of EF Core - for complex date functions, raw SQL is often clearer
        // Using parameterized query to prevent SQL injection
        var sql = @"
            SELECT
                DATEPART(YEAR, CompletedAt) AS [Year],
                DATEPART(WEEK, CompletedAt) AS [WeekNumber],
                COUNT(*) AS [CompletedTasksCount]
            FROM Tasks
            WHERE
                CompletedAt IS NOT NULL
                AND CompletedAt >= {0}
                AND CompletedAt <= {1}
            GROUP BY
                DATEPART(YEAR, CompletedAt),
                DATEPART(WEEK, CompletedAt)
            ORDER BY
                [Year],
                [WeekNumber]";

        // Use Database.SqlQueryRaw for DTOs that aren't entities
        // This is the correct way to execute raw SQL that returns non-entity types
        var result = await _context.Database
            .SqlQueryRaw<CompletedTasksPerWeekReportDto>(sql, startDate, endDate)
            .ToListAsync(cancellationToken);

        return result;
    }
}

