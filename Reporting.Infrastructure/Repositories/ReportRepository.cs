using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Repositories;

public sealed class TasksPerUserReportRepository
    : ITasksPerUserReportRepository
{
    private readonly ReportingDbContext _context;

    public TasksPerUserReportRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<TasksPerUserReportDto>> GetTasksPerUser_LinqAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var tasks = _context.Tasks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            tasks = tasks.Where(t => t.Status == status);

        if (from.HasValue)
            tasks = tasks.Where(t => t.CreatedAt >= from.Value);

        if (to.HasValue)
            tasks = tasks.Where(t => t.CreatedAt <= to.Value);

        return await tasks
            .GroupBy(t => new { t.UserId, t.User.Name })
            .Select(g => new TasksPerUserReportDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                TasksCount = g.Count()
            })
            .OrderByDescending(x => x.TasksCount)
            .ToListAsync();
    }

    public async Task<List<TasksPerUserReportDto>> GetTasksPerUser_RawSqlAsync(
        string? status = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var sql = """
        SELECT 
            u.Id AS UserId,
            u.Name AS UserName,
            COUNT(t.Id) AS TasksCount
        FROM Tasks t
        JOIN Users u ON t.UserId = u.Id
        WHERE 1 = 1
        """;

        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            sql += " AND t.Status = @status";
            parameters.Add(new SqlParameter("@status", status));
        }

        if (from.HasValue)
        {
            sql += " AND t.CreatedAt >= @from";
            parameters.Add(new SqlParameter("@from", from.Value));
        }

        if (to.HasValue)
        {
            sql += " AND t.CreatedAt <= @to";
            parameters.Add(new SqlParameter("@to", to.Value));
        }

        sql += " GROUP BY u.Id, u.Name ORDER BY TasksCount DESC";

        return await _context.Set<TasksPerUserReportDto>()
            .FromSqlRaw(sql, parameters.ToArray())
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(CompletedTasksPerWeekQuery query)
    {
        var sql = @"
            SELECT
                DATEPART(YEAR, CompletedAt)      AS [Year],
                DATEPART(WEEK, CompletedAt)      AS [WeekNumber],
                COUNT(*)                         AS [CompletedTasksCount]
            FROM Tasks
            WHERE
                CompletedAt IS NOT NULL
                AND CompletedAt >= @StartDate
                AND CompletedAt <= @EndDate
            GROUP BY
                DATEPART(YEAR, CompletedAt),
                DATEPART(WEEK, CompletedAt)
            ORDER BY
                [Year],
                [WeekNumber];

            ";

        var parameters = new[]
        {
    new SqlParameter("@StartDate", query.StartDate),
    new SqlParameter("@EndDate", query.EndDate)
};

        return await _context
            .Set<CompletedTasksPerWeekReportDto>()
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync();

    }
}
