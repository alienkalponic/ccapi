using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class CreatePersonRequestDto
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(150, ErrorMessage = "FullName cannot exceed 150 characters")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
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

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePersonRequestDto
    {
        [Required(ErrorMessage = "PersonId is required")]
        public long PersonId { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        [StringLength(150, ErrorMessage = "FullName cannot exceed 150 characters")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
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

        public bool IsActive { get; set; } = true;
    }
}
