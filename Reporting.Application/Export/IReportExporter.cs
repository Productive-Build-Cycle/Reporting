namespace Reporting.Application.Interfaces;

public interface IReportExporter
{
    string ReportType { get; }

    Task<byte[]> ExportAsync(HttpContext http, CancellationToken ct);
}

