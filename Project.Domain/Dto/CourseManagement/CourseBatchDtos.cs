using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class CreateCourseBatchRequestDto
    {
        [Required(ErrorMessage = "CourseId is required")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "CourseYear is required")]
        [Range(2000, 2100, ErrorMessage = "CourseYear must be between 2000 and 2100")]
        public int CourseYear { get; set; }

        public DateTime? CourseStartDate { get; set; }

        public DateTime? CourseEndDate { get; set; }

        [Range(0, 1000000, ErrorMessage = "TotalCourseFee must be non-negative")]
        public decimal TotalCourseFee { get; set; }

        [Range(0, 1000000, ErrorMessage = "AdvanceAmount must be non-negative")]
        public decimal AdvanceAmount { get; set; }

        [Range(0, 1000000, ErrorMessage = "CancellationRetentionAmount must be non-negative")]
        public decimal CancellationRetentionAmount { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCourseBatchRequestDto
    {
        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        [Required(ErrorMessage = "CourseId is required")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "CourseYear is required")]
        [Range(2000, 2100, ErrorMessage = "CourseYear must be between 2000 and 2100")]
        public int CourseYear { get; set; }

        public DateTime? CourseStartDate { get; set; }

        public DateTime? CourseEndDate { get; set; }

        [Range(0, 1000000, ErrorMessage = "TotalCourseFee must be non-negative")]
        public decimal TotalCourseFee { get; set; }

        [Range(0, 1000000, ErrorMessage = "AdvanceAmount must be non-negative")]
        public decimal AdvanceAmount { get; set; }

        [Range(0, 1000000, ErrorMessage = "CancellationRetentionAmount must be non-negative")]
        public decimal CancellationRetentionAmount { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
