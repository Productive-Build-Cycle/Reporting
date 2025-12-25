using Microsoft.EntityFrameworkCore;
using Reporting.Application.DTOs;
using Reporting.Infrastructure.Db;

namespace Reporting.Infrastructure.Repositories;

public sealed class TasksPerUserReportRepository
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
        var tasks = _context.Tasks
            .AsNoTracking()
            .AsQueryable();

        if(!string.IsNullOrWhiteSpace(status))
            tasks = tasks.Where(t => t.Status == status);

        if (to.HasValue)
            tasks = tasks.Where(t => t.CreatedAt >= from.Value);

        if (from.HasValue)
            tasks = tasks.Where(t => t.CreatedAt <= to.Value);

        return await tasks
            .GroupBy(t => t.UserId)
            .Select(g => new TasksPerUserReportDto
            {
                UserId = g.Key,
                UserName = g.Select(x => x.User.Name).FirstOrDefault(),
                TasksCount = g.Count()
            })
            .OrderByDescending(x => x.TasksCount)
            .ToListAsync();

    }
}