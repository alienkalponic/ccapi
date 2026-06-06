using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class ActivitieDetailsImage
    {
        [Key]
        public long? ActivitieDetailsImageId { get; set; }
        public long? ActivitieDetailsId { get; set; }
        public string? ActivitieDetailsImageName { get; set; }
        public string? ImagePath1 { get; set; }
        public short? DisplayPriority { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public long? DeletedBy { get; set; }
    }
}
