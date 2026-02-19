using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.Banner
{
    public class UpdateBannerDto
    {
        [Required]
        public long BannerId { get; set; }

        public IFormFile? File { get; set; }          // optional replace
        public IFormFile? MobileFile { get; set; }    // optional replace

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Caption { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
