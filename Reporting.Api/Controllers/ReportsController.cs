using Microsoft.AspNetCore.Mvc;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        try
        {
            var result = await _reportService.GetTasksPerUserAsync(query, cancellationToken);
            if (result.Count == 0)
                return NoContent();
            return Ok(result);

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving tasks per user reports.", message = ex.Message });
        }
    }

    /// <summary>
    /// Returns a weekly report of completed tasks based on the specified filters.
    /// </summary>
    /// <param name="query">
    /// Filter parameters including date range, team/user filters,
    /// and pagination settings such as PageNumber and PageSize.
    /// </param>
    /// <returns>
    /// A paginated weekly breakdown of completed tasks.
    /// </returns>
    /// <response code="200">Returns the weekly completed tasks report</response>
    /// <response code="500">Returned when an unexpected error occurs</response>

    [HttpGet("completed-tasks-per-week")]
    public async Task<IActionResult> GetCompletedTasksPerWeek([FromQuery] CompletedTasksPerWeekQueryDto query)
    {
        try
        {
            var result = await _reportService
            .GetCompletedTasksPerWeekAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "An error occurred while retrieving team performance summary",
                message = ex.Message
            });
        }
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
        try
        {
            var result = await _reportService.GetTeamPerformanceSummaryAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving team performance summary", message = ex.Message });
        }
    }

    // Excel Export API � Tasks Per User
    [HttpGet("tasks-per-user/export")]
    public async Task<IActionResult> ExportTasksPerUser(
        [FromQuery] TasksPerUserQueryDto query,
        [FromServices] ExcelExporter exporter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            query.PageNumber = 1;
            query.PageSize = int.MaxValue;

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
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while exporting tasks per user report.", message = ex.Message });
        }
    }

    // Excel Export API � Completed Tasks Per Week
    [HttpGet("completed-tasks-per-week/export")]
    public async Task<IActionResult> ExportCompletedTasksPerWeek(
        [FromQuery] CompletedTasksPerWeekQueryDto query,
        [FromServices] ExcelExporter exporter)
    {
        var result = await _reportService.GetCompletedTasksPerWeekAsync(query);

        if (result.Items.Count == 0)
            return NoContent();

        var file = exporter.ExportCompletedTasksPerWeek(result.Items);

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
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while exporting team performance summary", message = ex.Message });
        }
    }


}
