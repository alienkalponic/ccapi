using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class CreateCourseRequestDto
    {
        [Required(ErrorMessage = "CourseCode is required")]
        [StringLength(50, ErrorMessage = "CourseCode cannot exceed 50 characters")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "CourseName is required")]
        [StringLength(100, ErrorMessage = "CourseName cannot exceed 100 characters")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCourseRequestDto
    {
        [Required(ErrorMessage = "CourseId is required")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "CourseCode is required")]
        [StringLength(50, ErrorMessage = "CourseCode cannot exceed 50 characters")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "CourseName is required")]
        [StringLength(100, ErrorMessage = "CourseName cannot exceed 100 characters")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
