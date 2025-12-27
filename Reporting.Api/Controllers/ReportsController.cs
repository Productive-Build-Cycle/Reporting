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
}
