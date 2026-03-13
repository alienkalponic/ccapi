using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ClubDescription
{
    public class UpdateClubDescriptionImageDto
    {
        public long? ClubDescriptionImageId { get; set; }

        public string? Title { get; set; }

        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile File { get; set; } = default!;

        public int DisplayOrder { get; set; } = 0;
    }
}
