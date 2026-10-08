using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Dto.CourseManagement
{
    public class CreateCourseExpenseCategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCourseExpenseCategoryDto
    {
        [Required(ErrorMessage = "CourseExpenseCategoryId is required")]
        [Range(1, long.MaxValue, ErrorMessage = "CourseExpenseCategoryId must be greater than zero")]
        public long CourseExpenseCategoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
