using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class AddCourseExpenseRequestDto
    {
        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "ExpenseCategory is required")]
        [StringLength(100)]
        public string ExpenseCategory { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaidBy { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class UpdateCourseExpenseRequestDto
    {
        [Required(ErrorMessage = "ExpenseId is required")]
        public long ExpenseId { get; set; }

        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "ExpenseCategory is required")]
        [StringLength(100)]
        public string ExpenseCategory { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaidBy { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
