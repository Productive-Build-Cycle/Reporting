using Microsoft.AspNetCore.Mvc;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Returns number of tasks per user
    /// </summary>
    [HttpGet("tasks-per-user")]
    public async Task<IActionResult> GetTasksPerUser(
        [FromQuery] TasksPerUserQuery query)
    {
        var result = await _reportService.GetTasksPerUserAsync(query);

        if (result.Count == 0)
            return NoContent();

        return Ok(result);
    }

    // Excel Export API – Tasks Per User
    [HttpGet("tasks-per-user/export")]
    public async Task<IActionResult> ExportTasksPerUser(
        [FromQuery] TasksPerUserQuery query,
        [FromServices] ExcelExporter exporter)
    {
        var result = await _reportService.GetTasksPerUserAsync(query);

        if (result.Count == 0)
            return NoContent();

        var file = exporter.ExportTasksPerUser(result);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "tasks-per-user.xlsx"
        );
    }

    // Excel Export API – Completed Tasks Per Week
    [HttpGet("completed-tasks-per-week/export")]
    public async Task<IActionResult> ExportCompletedTasksPerWeek(
        [FromQuery] CompletedTasksPerWeekQueryDto query,
        [FromServices] ExcelExporter exporter)
    {
        var result = await _reportService.GetCompletedTasksPerWeekAsync(query);

        if (result.Count == 0)
            return NoContent();

        var file = exporter.ExportCompletedTasksPerWeek(result);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "completed-tasks-per-week.xlsx"
        );
    }

    // Excel Export API – Team Performance Summary
    [HttpGet("team-performance-summary/export")]
    public async Task<IActionResult> ExportTeamPerformanceSummary(
        [FromQuery] TeamPerformanceSummaryRequestDto query,
        [FromServices] ExcelExporter exporter,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTeamPerformanceSummaryAsync(query, cancellationToken);

        if (result.Count == 0)
            return NoContent();

        var file = exporter.ExportTeamPerformanceSummary(result);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "team-performance-summary.xlsx"
        );
    }


}
