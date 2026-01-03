using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Repositories;

public sealed class ReportRepository
    : IReportRepository
{
    private readonly ReportingDbContext _context;

    public ReportRepository(ReportingDbContext context)
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

    // Gets weekly completed tasks from the GetCompletedTasksPerWeek stored procedure.
    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(DateTime startDate, DateTime endDate)
    {
        return await _context
        .Set<CompletedTasksPerWeekReportDto>()
        .FromSqlRaw(
            "EXEC GetCompletedTasksPerWeek @StartDate, @EndDate",
            new SqlParameter("@StartDate", startDate),
            new SqlParameter("@EndDate", endDate)
        )
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<List<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tasksQuery = _context.Tasks.AsQueryable();

        // Apply date filters if provided
        if (request.StartDate.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        // Apply team filter if provided
        if (request.TeamId.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.TeamId == request.TeamId.Value);
        }

        // Join with Teams and group by team to calculate metrics
        var result = await (from task in tasksQuery
                           join team in _context.Teams on task.TeamId equals team.Id
                           group task by new { team.Id, team.Name } into g
                           select new TeamPerformanceSummaryResponseDto
                           {
                               TeamId = g.Key.Id,
                               TeamName = g.Key.Name,
                               TotalTasks = g.Count(),
                               CompletedTasks = g.Count(t => t.Status == "Completed" || t.CompletedAt != null),
                               CompletionRate = g.Count() > 0
                                   ? (decimal)g.Count(t => t.Status == "Completed" || t.CompletedAt != null) * 100 / g.Count()
                                   : 0
                           })
            .ToListAsync(cancellationToken);

        return result;
    }

}
