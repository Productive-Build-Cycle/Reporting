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

    // Excel Export API 
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


}
