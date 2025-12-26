using Microsoft.AspNetCore.Mvc;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("tasks-per-user")]
    [ProducesResponseType(typeof(List<TasksPerUserRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TasksPerUserRequestDto>>> GetTasksPerUserAsync(
    [FromQuery] bool useRawSql = false,
    [FromQuery] string? status = null,
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null)
    {
        if (from is not null && to is not null && from > to)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "'from' must be less than or equal to 'to'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _reportService.GetTasksPerUserAsync(
            useRawSql,
            status,
            from,
            to);

        return Ok(result);
    }

}