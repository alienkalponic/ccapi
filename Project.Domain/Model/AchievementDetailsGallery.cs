using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class AchievementDetailsGallery
    {
        [Key]
        public long AchievementDetailsGalleryId { get; set; }
        public long? AchievementDetailsId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImagePath1 { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
