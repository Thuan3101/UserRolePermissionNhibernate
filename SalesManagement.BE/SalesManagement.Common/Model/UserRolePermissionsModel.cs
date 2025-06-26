using SalesManagement.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Model
{
    public class UserRolePermissionsModel
    {
        public int UserId { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        //Respone không trả password
        public string Password { get; set; } 

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string RoleName { get; set; } // Giả sử mỗi user chỉ có 1 role
        public List<string> Permissions { get; set; } = new();


    }

    //public class UserRoleModel
    //{
    //    public int UserRoleId { get; set; } // Primary Key

    //    // Foreign Key mapping
    //    public UserModel User { get; set; }
    //    public RoleModel Role { get; set; }
    //}

    //public class RoleModel1
    //{
    //    public  int RoleId { get; set; }  // Changed from Id to RoleId for consistency
    //    public  string? RoleName { get; set; }
    //    public  string? Description { get; set; }
    //    public  ICollection<UserRoleModel> UserRolesModel { get; set; }
    //    public  ICollection<RolePermissionModel> RolePermissionsModel { get; set; }

    //    public RoleModel1()
    //    {
    //        UserRolesModel = new List<UserRoleModel>();
    //        RolePermissionsModel = new List<RolePermissionModel>();
    //    }
    //}
    //public class RolePermissionModel
    //{
    //    public int RolePermissionId { get; set; }
    //    public Role Role { get; set; }
    //    public PermissionModel1 PermissionModel1 { get; set; }
    //}
    //public class PermissionModel1
    //{
    //    public int PermissionId { get; set; } // Phải trùng tên với XML mapping và DB
    //    public string? PermissionName { get; set; }
    //    public string? Description { get; set; } // Có cột Description trong SQL
    //    public ICollection<RolePermissionModel> RolePermissionModel { get; set; } = new List<RolePermissionModel>();
    //}
}
