using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class AddCourseOfficialRequestDto
    {
        [Required(ErrorMessage = "Official Name is required")]
        public string? CourseOfficialName { get; set; }

        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        [Required(ErrorMessage = "CourseId is required")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "RoleName is required")]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? HowrahToDestination { get; set; }

        [StringLength(100)]
        public string? DestinationToHowrah { get; set; }

        [Range(0, 1000000, ErrorMessage = "Amount must be non-negative")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "PENDING";

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class UpdateCourseOfficialRequestDto
    {
        [Required(ErrorMessage = "CourseOfficialId is required")]
        public long CourseOfficialId { get; set; }

        [Required(ErrorMessage = "Official Name is required")]
        public string? CourseOfficialName { get; set; }

        [Required(ErrorMessage = "CourseId is required")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "PersonId is required")]
        public long PersonId { get; set; }

        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        [Required(ErrorMessage = "RoleName is required")]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? HowrahToDestination { get; set; }

        [StringLength(100)]
        public string? DestinationToHowrah { get; set; }

        [Range(0, 1000000, ErrorMessage = "Amount must be non-negative")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "PENDING";

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class UpdateOfficialPaymentStatusDto
    {
        [Required(ErrorMessage = "CourseOfficialId is required")]
        public long CourseOfficialId { get; set; }

        [Required(ErrorMessage = "PaymentStatus is required")]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "PENDING";

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
