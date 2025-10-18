using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubfig.Core.Entities
{
    public  class User
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public string? Title { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        // Navigation
        public Organization? Organization { get; set; }
    }
}
