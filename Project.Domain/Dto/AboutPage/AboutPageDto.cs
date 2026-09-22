using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Project.Domain.Dto.AboutPage
{
    public class AboutPageDto
    {
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
        public bool? IsActive { get; set; }

        [JsonIgnore]
        public IFormFile? BannerFile { get; set; }

        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }

    public class AboutPageSectionDto
    {
        public string? SectionType { get; set; }
        public string? SectionTitle { get; set; }
        public string? SectionSubtitle { get; set; }
        public string? SectionDescription { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsVisible { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class AboutDetailsDto
    {
        public long? AboutDetailsId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }

        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class AboutPersonDto
    {
        public long? PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? FullDescription { get; set; }
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }

        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class CreateAboutPageDto
    {
        public AboutPageDto? AboutPage { get; set; }

        [JsonIgnore]
        public IFormFile? BannerFile { get; set; }

        [JsonIgnore]
        public IFormFile? File { get; set; }

        public List<AboutPageSectionDto>? AboutPageSection { get; set; }

        public List<AboutDetailsDto>? AboutDetails { get; set; }

        public List<AboutPersonDto>? AboutPerson { get; set; }

        public long? CreatedBy { get; set; }
    }

    public class UpdateAboutPageDto
    {
        public long AboutPageId { get; set; }

        public AboutPageDto? AboutPage { get; set; }

        [JsonIgnore]
        public IFormFile? BannerFile { get; set; }

        [JsonIgnore]
        public IFormFile? File { get; set; }

        public List<AboutPageSectionDto>? AboutPageSection { get; set; }

        public List<AboutDetailsDto>? AboutDetails { get; set; }

        public List<AboutPersonDto>? AboutPerson { get; set; }

        public long? UpdatedBy { get; set; }

        public long? DeletedBy { get; set; }
    }
}
