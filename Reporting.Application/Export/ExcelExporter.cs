using OfficeOpenXml;
using OfficeOpenXml.Style;
using Reporting.Application.DTOs;

public class ExcelExporter
{
    public byte[] ExportTasksPerUser(List<TasksPerUserReportDto> data)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
}
