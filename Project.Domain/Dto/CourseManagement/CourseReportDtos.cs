using System;

namespace Project.Domain.Dto.CourseManagement
{
    public class CourseDashboardRequestDto
    {
        public long? CourseId { get; set; }
        public long? CourseBatchId { get; set; }
        public int? CourseYear { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class YearWiseSummaryRequestDto
    {
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
    }
}
