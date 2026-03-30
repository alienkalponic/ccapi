using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ClubActivity
{
    public class CreateActivityDto
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public string? RedirectUrl { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
