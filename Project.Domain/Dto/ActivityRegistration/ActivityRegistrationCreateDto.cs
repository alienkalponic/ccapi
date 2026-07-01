using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ActivityRegistration
{
    public class ActivityRegistrationCreateDto
    {
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public long? ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? MessageNotes { get; set; }
        public string? ApplicationStatus { get; set; }
        public bool? IsDeleted { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
