using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ClubDescription
{
    public class UpdateClubDescriptionDto
    {
        public long ClubDescriptionId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }

        public string? ImagesJson { get; set; }   // metadata
        public List<IFormFile>? Files { get; set; }
    }
}
