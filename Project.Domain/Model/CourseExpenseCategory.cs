using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Model
{
    public class CourseExpenseCategory
    {
        [Key]
        public long CourseExpenseCategoryId { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
