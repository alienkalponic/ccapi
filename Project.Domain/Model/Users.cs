using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class Users
    {
        [Key]
        public long UserId { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public long ContactId { get; set; }
        public string Comments { get; set; }
        public bool IsVerificationNeeded { get; set; }
        public string VerificationLinkId { get; set; }
        public string VerificationLink { get; set; }
        public bool EmailOwnershipVerificationStatus { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public long DeletedBy { get; set; }
        public DateTime? LastLogin { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool TwoWayVerification { get; set; }

    }
}
