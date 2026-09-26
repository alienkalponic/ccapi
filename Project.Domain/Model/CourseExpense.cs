using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CourseExpense")]
    public class CourseExpense
    {
        [Key]
        public long ExpenseId { get; set; }

        public long CourseBatchId { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        public string ExpenseCategory { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaidBy { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
