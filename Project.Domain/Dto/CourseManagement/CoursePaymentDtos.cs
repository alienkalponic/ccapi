using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class AddCoursePaymentRequestDto
    {
        [Required(ErrorMessage = "EnrollmentId is required")]
        public long EnrollmentId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(100)]
        public string? PaymentReceiver { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class UpdateCoursePaymentRequestDto
    {
        [Required(ErrorMessage = "PaymentId is required")]
        public long PaymentId { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than zero")]
        public decimal? Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(100)]
        public string? PaymentReceiver { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
