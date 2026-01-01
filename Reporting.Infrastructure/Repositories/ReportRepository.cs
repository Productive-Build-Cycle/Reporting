using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Export.HtmlExport.StyleCollectors.StyleContracts;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;
using System.Data;
using System.Runtime.InteropServices;

namespace Reporting.Infrastructure.Repositories;

public sealed class ReportRepository
    : IReportRepository
{
    private readonly ReportingDbContext _context;

    public ReportRepository(ReportingDbContext context)
    {
        _context = context;
    }

    // Tasks Per User By DAPPER
    public async Task<List<TasksPerUserReportDto>> GetTasksPerUserDapperAsync(
    TasksPerUserQueryDto query,
    CancellationToken cancellationToken)
    {
        const string baseSql = """
            SELECT 
                u.Id   AS UserId,
                u.Name AS UserName,
                COUNT(t.Id) AS TasksCount
            FROM Tasks t
            JOIN Users u ON t.UserId = u.Id
            WHERE 1 = 1
            """;

        var filters = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            filters.Add("AND t.Status = @Status");
            parameters.Add("Status", query.Status);
        }

        if (query.From.HasValue)
        {
            filters.Add("AND t.CreatedAt >= @From");
            parameters.Add("From", query.From.Value);
        }

        if (query.To.HasValue)
        {
            filters.Add("AND t.CreatedAt <= @To");
            parameters.Add("To", query.To.Value);
        }

        var sql = $@"
        {baseSql}
        {string.Join(" ", filters)}
        GROUP BY u.Id, u.Name
        ORDER BY TasksCount DESC";

        using var conn = new SqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(cancellationToken);

        var result = await conn.QueryAsync<TasksPerUserReportDto>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return result.ToList();
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