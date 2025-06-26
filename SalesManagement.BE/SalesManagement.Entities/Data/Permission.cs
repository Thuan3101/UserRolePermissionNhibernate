using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace SalesManagement.Entities.Data
{
    public class Permission
    {
        public virtual int PermissionId { get; set; } // Phải trùng tên với XML mapping và DB
        public virtual string PermissionCode { get; set; } // Mã quyền, có thể là duy nhất
        public virtual string? PermissionName { get; set; }
        public virtual string? Description { get; set; } // Có cột Description trong SQL
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}

