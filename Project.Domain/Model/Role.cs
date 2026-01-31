using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class Role
    {
        [Key]
        public long RoleId { get; set; }
        public string? RoleName { get; set; }
        public short? DisplayPriority { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public long? DeletedBy { get; set; }
    }
}
