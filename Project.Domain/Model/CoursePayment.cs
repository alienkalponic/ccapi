using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CoursePayment")]
    public class CoursePayment
    {
        [Key]
        public long PaymentId { get; set; }

        public long EnrollmentId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; }

        [StringLength(100)]
        public string? PaymentReceiver { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
