using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Repositories;

public class PerformanceReportRepository : IPerformanceRepository
{
    private readonly ReportingDbContext _context;

    public PerformanceReportRepository(ReportingDbContext context)
    {
        _context = context;
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

