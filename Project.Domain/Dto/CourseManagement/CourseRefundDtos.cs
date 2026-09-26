using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class CancelEnrollmentRequestDto
    {
        [Required(ErrorMessage = "EnrollmentId is required")]
        public long EnrollmentId { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        [StringLength(50)]
        public string? RefundMode { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }
    }

    public class AddCourseRefundRequestDto
    {
        [Required(ErrorMessage = "EnrollmentId is required")]
        public long EnrollmentId { get; set; }

        public DateTime RefundDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "RefundAmount is required")]
        [Range(0.01, 1000000, ErrorMessage = "RefundAmount must be greater than zero")]
        public decimal RefundAmount { get; set; }

        [StringLength(50)]
        public string? RefundMode { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }
    }
}
