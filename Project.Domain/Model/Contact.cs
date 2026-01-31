using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Model
{
    public class Contact
    {
        [Key]
        public long ContactId { get; set; }
        public long RoleId { get; set; }
        public string? MSHClientNumber { get; set; }
        public string? TitleId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNo1 { get; set; }
        public string? PhoneNo2 { get; set; }
        public string? EmailAddress { get; set; }
        public string? EmailAddress1 { get; set; }
        public string? JobTitle { get; set; }
        public string? ContactNote { get; set; }
        public short MonthOfBirth { get; set; }
        public short YearOfBirth { get; set; }
        public string? Gender { get; set; }
        public long? AssignedToUserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public long? DeletedBy { get; set; }

    }
}
