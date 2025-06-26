using System;

namespace SalesManagement.Entities.Data
{
    public class RolePermission
    {
        public virtual int RolePermissionId { get; set; }
        public virtual string PermissionCode { get; set; } // Unique code for the permission, if needed
        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (RolePermission)obj;
            return RolePermissionId == other.RolePermissionId &&
                   ((Role?.RoleId == other.Role?.RoleId) || (Role == null && other.Role == null)) &&
                   ((Permission?.PermissionId == other.Permission?.PermissionId) || (Permission == null && other.Permission == null));
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + RolePermissionId.GetHashCode();
            hash = hash * 23 + (Role?.RoleId ?? 0).GetHashCode();
            hash = hash * 23 + (Permission?.PermissionId ?? 0).GetHashCode();
            return hash;
        }
    }
}