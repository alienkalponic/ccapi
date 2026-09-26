using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CourseBatch")]
    public class CourseBatch
    {
        [Key]
        public long CourseBatchId { get; set; }

        public long CourseId { get; set; }

        public int? CourseYear { get; set; }

        public DateTime? CourseStartDate { get; set; }

        public DateTime? CourseEndDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalCourseFee { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal AdvanceAmount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CancellationRetentionAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
