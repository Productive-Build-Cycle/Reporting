using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Application.DTOs
{
    public class CompletedTasksPerWeekQuery
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
