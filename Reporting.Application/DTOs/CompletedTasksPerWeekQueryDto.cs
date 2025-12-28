using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Application.DTOs
{
    public class CompletedTasksPerWeekQueryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
