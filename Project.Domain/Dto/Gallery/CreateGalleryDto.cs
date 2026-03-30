using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.Gallery
{
    public class CreateGalleryDto
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public int? ExpeditionYear { get; set; }
        public IFormFile? File { get; set; }
        public int DisplayOrder { get; set; }
    }
}
