using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Application.DTOs
{
    public class CompletedTasksPerWeekQueryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
