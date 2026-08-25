using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Dto.ActivityRegistration
{
    public class CreateActivityInterestRegistrationDto
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public long ClubActivityId { get; set; }

        public string? Message { get; set; }
    }
}
