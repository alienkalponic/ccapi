using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class Category
    {
        [Key]
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public short? DisplayPriority { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public long? DeletedBy { get; set; }
    }
}
