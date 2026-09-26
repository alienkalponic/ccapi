using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class PersonSubDto
    {
        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(150)]
        public string? FatherMotherName { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Profession { get; set; }

        [StringLength(50)]
        public string? MotherTongue { get; set; }

        [StringLength(20)]
        public string? Height { get; set; }

        [StringLength(20)]
        public string? Weight { get; set; }

        [StringLength(10)]
        public string? BloodGroup { get; set; }

        [StringLength(50)]
        public string? FoodHabit { get; set; }

        [StringLength(500)]
        public string? PhysicalProblem { get; set; }
    }

    public class CreateCourseEnrollmentRequestDto
    {
        // Support nested Person object
        public PersonSubDto? Person { get; set; }

        // Or flat Person properties
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? FatherMotherName { get; set; }
        public string? Address { get; set; }
        public string? Profession { get; set; }
        public string? MotherTongue { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? BloodGroup { get; set; }
        public string? FoodHabit { get; set; }
        public string? PhysicalProblem { get; set; }

        // Enrollment details
        [Required(ErrorMessage = "CourseBatchId is required")]
        public long CourseBatchId { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        [StringLength(100)]
        public string? ReferencePerson { get; set; }

        public bool FormSubmitted { get; set; } = true;

        [StringLength(50)]
        public string Status { get; set; } = "REGISTERED";

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Optional payment details
        public decimal? InitialPaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaymentReceiver { get; set; }
        public string? TransactionReference { get; set; }
    }

    public class UpdateCourseEnrollmentRequestDto
    {
        [Required(ErrorMessage = "EnrollmentId is required")]
        public long EnrollmentId { get; set; }

        public PersonSubDto? Person { get; set; }

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? FatherMotherName { get; set; }
        public string? Address { get; set; }
        public string? Profession { get; set; }
        public string? MotherTongue { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? BloodGroup { get; set; }
        public string? FoodHabit { get; set; }
        public string? PhysicalProblem { get; set; }

        public long CourseBatchId { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        [StringLength(100)]
        public string? ReferencePerson { get; set; }

        public bool FormSubmitted { get; set; } = true;

        [StringLength(50)]
        public string EnrollmentStatus { get; set; } = "REGISTERED";

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Optional Payment details (nested or flat)
        public PaymentSubDto? Payment { get; set; }
        public long? PaymentId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaymentReceiver { get; set; }
        public string? TransactionReference { get; set; }
        public string? PaymentRemarks { get; set; }
    }

    public class PaymentSubDto
    {
        public long? PaymentId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaymentReceiver { get; set; }
        public string? TransactionReference { get; set; }
        public string? Remarks { get; set; }
    }
}
