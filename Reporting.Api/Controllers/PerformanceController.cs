using Microsoft.AspNetCore.Mvc;
using Reporting.Application.DTOs;
using Reporting.Application.Interfaces;

namespace Reporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly IReportService _reportService;

    public PerformanceController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Get Team Performance Summary Report
    /// گزارش خلاصه عملکرد تیم‌ها
    /// </summary>
    /// <param name="request">Filter parameters (StartDate, EndDate, TeamId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of team performance summaries with metrics:
    /// - Total Tasks (تعداد کل تسک‌ها)
    /// - Completed Tasks (تعداد تسک‌های تکمیل‌شده)
    /// - Completion Rate (نرخ تکمیل)
    /// </returns>
    [HttpGet("team-performance-summary")]
    [ProducesResponseType(typeof(List<TeamPerformanceSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TeamPerformanceSummaryResponseDto>>> GetTeamPerformanceSummary(
        [FromQuery] TeamPerformanceSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTeamPerformanceSummaryAsync(request, cancellationToken);
        return Ok(result);
    }
}

