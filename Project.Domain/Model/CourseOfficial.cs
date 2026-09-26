using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CourseOfficial")]
    public class CourseOfficial
    {
        [Key]
        public long CourseOfficialId { get; set; }

        public long PersonId { get; set; }

        public long CourseBatchId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? HowrahToDestination { get; set; }

        [StringLength(100)]
        public string? DestinationToHowrah { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "PENDING";

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
