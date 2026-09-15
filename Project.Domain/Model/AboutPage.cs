using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class AboutPage
    {
        [Key]
        public long AboutPageId { get; set; }
        public string? PageTitle { get; set; }
        public string? PageSlug { get; set; }

        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HistoryTitle { get; set; }
        public string? HistoryDescription { get; set; }
        public string? MapTitle { get; set; }
        public string? MapAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? BannerImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public long? DeletedBy { get; set; }
    }
}
