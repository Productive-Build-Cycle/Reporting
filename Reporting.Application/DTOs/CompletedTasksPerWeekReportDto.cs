using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Application.DTOs
{
    public class CompletedTasksPerWeekReportDto
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public int CompletedTasksCount { get; set; }
    }
}
