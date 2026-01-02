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
    /// Returns the number of tasks assigned per user within the specified filters.
    /// گزارش تعداد تسک‌های تخصیص‌یافته به هر کاربر
    /// </summary>
    /// <param name="query">Filter parameters including date range, user filters, etc.</param>
    /// <returns>
    /// A collection of user task counts. Returns 204 No Content if no tasks are found.
    /// </returns>
    /// <response code="200">Returns the list of users with their task counts</response>
    /// <response code="204">No tasks found for the given filters</response>
    [HttpGet("tasks-per-user")]
    [ProducesResponseType(typeof(List<TasksPerUserReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksPerUser(
        [FromQuery] TasksPerUserQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTasksPerUserAsync(query);
        if (result.Count == 0)
            return NoContent();
        return Ok(result);
    }

    /// <summary>
    /// Returns a weekly report of completed tasks.
    /// گزارش هفتگی تسک‌های تکمیل‌شده
    /// </summary>
    /// <param name="query">Filter parameters such as date range, team, or user filters</param>
    /// <returns>A weekly breakdown of completed tasks</returns>
    /// <response code="200">Returns the weekly completed tasks report</response>
    [HttpGet("completed-tasks-per-week")]
    [ProducesResponseType(typeof(List<CompletedTasksPerWeekReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompletedTasksPerWeek([FromQuery] CompletedTasksPerWeekQueryDto query)
    {
        var result = await _reportService.GetCompletedTasksPerWeekAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Team Performance Summary Report
    /// گزارش خلاصه عملکرد تیم‌ها
    /// </summary>
    /// <param name="request">Filter parameters (StartDate, EndDate, TeamId, PageNumber, PageSize)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of team performance summaries with metrics:
    /// - Total Tasks (تعداد کل تسک‌ها)
    /// - Completed Tasks (تعداد تسک‌های تکمیل‌شده)
    /// - Completion Rate (نرخ تکمیل)
    /// </returns>
    /// <response code="200">Returns the paginated list of team performance summaries</response>
    [HttpGet("team-performance-summary")]
    [ProducesResponseType(typeof(PagedResultDto<TeamPerformanceSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TeamPerformanceSummaryResponseDto>>> GetTeamPerformanceSummary(
        [FromQuery] TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTeamPerformanceSummaryAsync(request, cancellationToken);
        return Ok(result);
    }

    // Excel Export API � Tasks Per User
    [HttpGet("tasks-per-user/export")]
    public async Task<IActionResult> ExportTasksPerUser(
        [FromQuery] TasksPerUserQueryDto query,
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

    // Excel Export API � Completed Tasks Per Week
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

    // Excel Export API � Team Performance Summary
    [HttpGet("team-performance-summary/export")]
    public async Task<IActionResult> ExportTeamPerformanceSummary(
        [FromQuery] TeamPerformanceSummaryRequestDto query,
        [FromServices] ExcelExporter exporter,
        CancellationToken cancellationToken = default)
    {
        // For export, get all data by setting a large page size
        query.PageNumber = 1;
        query.PageSize = int.MaxValue;
        
        var result = await _reportService.GetTeamPerformanceSummaryAsync(query, cancellationToken);

        if (result.Items.Count == 0)
            return NoContent();

        var file = exporter.ExportTeamPerformanceSummary(result.Items);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "team-performance-summary.xlsx"
        );
    }


}
