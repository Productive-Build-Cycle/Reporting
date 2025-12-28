using OfficeOpenXml;
using OfficeOpenXml.Style;
using Reporting.Application.DTOs;

public class ExcelExporter
{
    // Tasks Per User Exporter
    public byte[] ExportTasksPerUser(List<TasksPerUserReportDto> data)
    {

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("TasksPerUser");

        // Header row
        ws.Cells["A1"].Value = "UserId";
        ws.Cells["B1"].Value = "UserName";
        ws.Cells["C1"].Value = "TasksCount";

        using (var range = ws.Cells["A1:C1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data rows
        for (int i = 0; i < data.Count; i++)
        {
            ws.Cells[i + 2, 1].Value = data[i].UserId;
            ws.Cells[i + 2, 2].Value = data[i].UserName;
            ws.Cells[i + 2, 3].Value = data[i].TasksCount;
        }

        ws.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    // Completed Tasks Per Week Exporter
    public byte[] ExportCompletedTasksPerWeek(List<CompletedTasksPerWeekReportDto> data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("CompletedTasksPerWeek");

        // Header
        ws.Cells["A1"].Value = "Week";
        ws.Cells["B1"].Value = "UserName";
        ws.Cells["C1"].Value = "CompletedTasks";

        using (var range = ws.Cells["A1:C1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        for (int i = 0; i < data.Count; i++)
        {
            ws.Cells[i + 2, 1].Value = data[i].WeekNumber;
            ws.Cells[i + 2, 2].Value = data[i].Year;
            ws.Cells[i + 2, 3].Value = data[i].CompletedTasksCount;
        }

        ws.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }


    // Team Performance Exporter
    public byte[] ExportTeamPerformanceSummary(List<TeamPerformanceSummaryResponseDto> data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("TeamPerformanceSummary");

        // Header
        ws.Cells["A1"].Value = "TeamId";
        ws.Cells["B1"].Value = "TeamName";
        ws.Cells["C1"].Value = "TotalTasks";
        ws.Cells["D1"].Value = "CompletedTasks";
        ws.Cells["E1"].Value = "CompletionRate";

        using (var range = ws.Cells["A1:E1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        for (int i = 0; i < data.Count; i++)
        {
            ws.Cells[i + 2, 1].Value = data[i].TeamId;
            ws.Cells[i + 2, 2].Value = data[i].TeamName;
            ws.Cells[i + 2, 3].Value = data[i].TotalTasks;
            ws.Cells[i + 2, 4].Value = data[i].CompletedTasks;
            ws.Cells[i + 2, 5].Value = data[i].CompletionRate;
        }

        ws.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}
