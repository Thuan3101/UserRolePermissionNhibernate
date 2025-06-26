using System;

namespace SalesManagement.Entities.Data
{
    public class UserRole
    {
        public virtual int UserRoleId { get; set; } // Primary Key

        // Foreign Key mapping
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (UserRole)obj;
            return UserRoleId == other.UserRoleId &&
                   ((User?.UserId == other.User?.UserId) || (User == null && other.User == null)) &&
                   ((Role?.RoleId == other.Role?.RoleId) || (Role == null && other.Role == null));
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + UserRoleId.GetHashCode();
            hash = hash * 23 + (User?.UserId ?? 0).GetHashCode();
            hash = hash * 23 + (Role?.RoleId ?? 0).GetHashCode();
            return hash;
        }
    }
}