using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Model
{
    [Table("CourseEnrollment")]
    public class CourseEnrollment
    {
        [Key]
        public long EnrollmentId { get; set; }

        public long PersonId { get; set; }

        public long CourseBatchId { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        [StringLength(100)]
        public string? ReferencePerson { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool FormSubmitted { get; set; } = true;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "REGISTERED";

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
