using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Model
{
    public class PermissionModel
    {
        public int PermissionId { get; set; } 
        public string PermissionCode { get; set; } // Unique code for the permission, if needed
        public string? PermissionName { get; set; }
        public string? Description { get; set; }
    }
}
