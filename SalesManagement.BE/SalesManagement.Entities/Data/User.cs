using System;
using System.Collections.Generic;

namespace SalesManagement.Entities.Data
{
    public class User
    {
        public virtual int UserId { get; set; }
        public virtual string UserCode { get; set; }
        public virtual string UserName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Email { get; set; }
        public virtual DateTime DateOfBirth { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Password { get; set; }
        public virtual string Status { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual int FailedLoginAttempts { get; set; } = 0;
        public virtual DateTime? LastFailedLoginAttempt { get; set; } = null;
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        public User()
        {
            UserRoles = new List<UserRole>();
            RefreshTokens = new List<RefreshToken>();
        }
    }
}
