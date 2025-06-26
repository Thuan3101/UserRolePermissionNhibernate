using AutoMapper;
using SalesManagement.Common.Model;
using SalesManagement.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Helper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<Role, RoleModel>().ReverseMap();
            CreateMap<Permission, PermissionModel>().ReverseMap();
            CreateMap<User, UserRolePermissionsModel>().ReverseMap();
            CreateMap<UserRolePermissionsModel, UserRole>().ReverseMap();
            CreateMap<UserRolePermissionsModel, Role>().ReverseMap();
            CreateMap<UserRolePermissionsModel, RolePermission>().ReverseMap();
            CreateMap<UserRolePermissionsModel, Permission>().ReverseMap();

            //CreateMap<UserRole, UserRoleModel>().ReverseMap();
            //CreateMap<Role, RoleModel1>().ReverseMap();
            //CreateMap<RolePermission, RolePermissionModel>().ReverseMap();
            //CreateMap<Permission, PermissionModel1>().ReverseMap();
        }
    }
}