using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;
using System.Data;

namespace Reporting.Infrastructure.Repositories;

public sealed class ReportRepository
    : IReportRepository
{
    private readonly ReportingDbContext _context;

    public ReportRepository(ReportingDbContext context)
    {
        _context = context;
    }

    // Tasks Per User By EfCoreLinq
    public async Task<List<TasksPerUserReportDto>> GetTasksPerUserEfAsync(
    TasksPerUserQueryDto query,
    CancellationToken cancellationToken)
    {
        var tasks = _context.Tasks
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            tasks = tasks.Where(t => t.Status == query.Status);
        }

        if (query.From.HasValue)
        {
            tasks = tasks.Where(t => t.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            tasks = tasks.Where(t => t.CreatedAt <= query.To.Value);
        }

        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var result = await tasks
            .GroupBy(t => new { t.UserId, t.User.Name })
            .Select(g => new TasksPerUserReportDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                TasksCount = g.Count()
            })
            .OrderByDescending(r => r.TasksCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return result;
    }


    public async Task<List<CompletedTasksPerWeekReportDto>> GetCompletedTasksPerWeekAsync(
            CompletedTasksPerWeekQueryDto query)
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

    public async Task<PagedResultDto<TeamPerformanceSummaryResponseDto>> GetTeamPerformanceSummaryAsync(
        TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Build base query with filters
        var tasksQuery = _context.Tasks.AsNoTracking().AsQueryable();

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
        var groupedQuery = from task in tasksQuery
                           join team in _context.Teams.AsNoTracking() on task.TeamId equals team.Id
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
                           };

        // Get total count before pagination
        var totalCount = await groupedQuery.CountAsync(cancellationToken);

        // Apply pagination
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var items = await groupedQuery
            .OrderByDescending(x => x.CompletionRate)
            .ThenByDescending(x => x.TotalTasks)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TeamPerformanceSummaryResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

}