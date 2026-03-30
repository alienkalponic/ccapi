using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ClubActivity
{
    public class UpdateActivityDto: CreateActivityDto
    {
        public long ActivityId { get; set; }
        public bool IsActive { get; set; }
    }
}
