using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Entities.Data
{
    public class Role
    {
        public virtual int RoleId { get; set; }  // Changed from Id to RoleId for consistency
        public virtual string? RoleName { get; set; }
        public virtual string? Description { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        public Role()
        {
            UserRoles = new List<UserRole>();
            RolePermissions = new List<RolePermission>();
        }
    }
}
