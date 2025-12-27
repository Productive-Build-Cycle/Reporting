using OfficeOpenXml;
using Reporting.Application.DTOs;

public class ExcelExporter
{
    public byte[] ExportTasksPerUser(List<TasksPerUserReportDto> data)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("TasksPerUser");

        //Header
        ws.Cells[1, 1].Value = "UserId";
        ws.Cells[1, 2].Value = "UserName";
        ws.Cells[1, 3].Value = "TasksCount";

        //Data
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