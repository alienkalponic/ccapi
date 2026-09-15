using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class AboutPageSection
    {
        [Key]
        public long SectionId { get; set; }
        public long AboutPageId { get; set; }
        public string? SectionType { get; set; }
        public string? SectionTitle { get; set; }
        public string? SectionSubtitle { get; set; }
        public string? SectionDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
