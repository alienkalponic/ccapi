using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CourseRefund")]
    public class CourseRefund
    {
        [Key]
        public long RefundId { get; set; }

        public long EnrollmentId { get; set; }

        public DateTime RefundDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal RefundAmount { get; set; }

        [StringLength(50)]
        public string? RefundMode { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
