using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Api.Authorization;
using SalesManagement.Api.Controllers.Parameters;
using SalesManagement.Bussiness;
using SalesManagement.Common.Model;

using SalesManagement.Common.Response;
using SalesManagement.Entities.Data;
using SalesManagement.Entities.Enum;

namespace SalesManagement.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserServiceBll _userService;

        public UserController(UserParameters userParameters)
        {

            _userService = userParameters.UserService;
        }

        //[HttpGet("{id}")]
        //public IActionResult GetUser(int id)
        //{
        //    try
        //    {
        //        var user = _userService.GetUserById(id);
        //        if (user == null)
        //        {
        //            return NotFound(new ApiResponseError
        //            {
        //                StatusCode = StatusCodes.Status404NotFound,
        //                Success = false,
        //                Message = "User not found.",
        //                Data = null
        //            });
        //        }
        //        return Ok(new ApiResponseSuccess<User>
        //        {
        //            StatusCode = StatusCodes.Status200OK,
        //            Success = true,
        //            Message = "User retrieved successfully.",
        //            Data = user
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
        //        {
        //            StatusCode = StatusCodes.Status500InternalServerError,
        //            Success = false,
        //            Message = "An error occurred while retrieving the user.",
        //            Error = ex.Message
        //        });
        //    }
        //}
        //[Authorize(Roles ="SuperAdmin")]
        [HttpGet("all")]
        
        
        public async Task<IActionResult> GetAllUsers(int pageSize = 10, int currentPage = 1, string? search = null)
        {
            try
            {
                var allUsers = await _userService.GetAllUsersAsync(pageSize, currentPage, search);

               

                if (allUsers == null || !allUsers.Any())
                {
                    return NotFound(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Success = false,
                        Message = "No users found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponseSuccessPaginated<List<UserModel>> 
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "Users retrieved successfully.",
                    Data = allUsers,
                    TotalCount = allUsers.Count, // Assuming you have total count logic in your service
                    PageSize = pageSize,
                    CurrentPage = currentPage

                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while retrieving all users."
                });
            }
        }
        //Get all roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles(int pageSize = 10, int currentPage = 1, string? search = null)
        {
            try
            {
                var allRoles = await _userService.GetAllRolesAsync(pageSize, currentPage, search);
                if (allRoles == null || !allRoles.Any())
                {
                    return NotFound(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Success = false,
                        Message = "No roles found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponseSuccessPaginated<List<RoleModel>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "Roles retrieved successfully.",
                    Data = allRoles,
                    TotalCount = allRoles.Count, 
                    PageSize = pageSize,
                    CurrentPage = currentPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while retrieving roles."
                });
            }
        }

        //Get all permissions
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions(int pageSize = 10, int currentPage = 1, string? search = null)
        {
            try
            {
                var allPermissions = await _userService.GetAllPermissionsAsync(pageSize, currentPage, search);
                if (allPermissions == null || !allPermissions.Any())
                {
                    return NotFound(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Success = false,
                        Message = "No permissions found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponseSuccessPaginated<List<PermissionModel>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "Permissions retrieved successfully.",
                    Data = allPermissions,
                    TotalCount = allPermissions.Count, 
                    PageSize = pageSize,
                    CurrentPage = currentPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while retrieving permissions."
                });
            }
        }

        //Get all users Roles permissions //UserRolePermissionsModel,UserRoleModel,RoleModel1,RolePermissionModel,PermissionModel1
        [Permission(PermissionEnum.VIEW_USERS)]
        [HttpGet("user-role-permissions")]
        public async Task<IActionResult> GetAllUserRolePermissions(
             int pageSize = 10,
             int currentPage = 1,
             [FromQuery] UserRolePermissionsSearchModel? search = null
         )
        {
            try
            {
                var allUserRolePermissions = await _userService.GetAllUserRolePermissionsAsync(pageSize, currentPage, search);

                if (allUserRolePermissions == null || !allUserRolePermissions.Any())
                {
                    return NotFound(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Success = false,
                        Message = "No user role permissions found.",
                        Data = null
                    });
                }

                return Ok(new ApiResponseSuccessPaginated<List<UserRolePermissionsSearchModel>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "User role permissions retrieved successfully.",
                    Data = allUserRolePermissions,
                    TotalCount = allUserRolePermissions.Count,
                    PageSize = pageSize,
                    CurrentPage = currentPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while retrieving user role permissions."
                });
            }
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserRolePermissionsModel model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "Dữ liệu người dùng không hợp lệ.",
                    Data = null
                });
            }

            try
            {
                // Validation cơ bản
                if (string.IsNullOrEmpty(model.UserName) ||
                    string.IsNullOrEmpty(model.Password) ||
                    string.IsNullOrEmpty(model.RoleName))
                {
                    return BadRequest(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Success = false,
                        Message = "Username, Password và Role không được để trống.",
                        Data = null
                    });
                }

                // Tạo user với role và permissions
                var createdUser = await _userService.CreateUserWithRoleAndPermissionsAsync(model);

                // Loại bỏ password trước khi trả về
                createdUser.Password = null;

                return CreatedAtAction(
                    nameof(GetAllUsers),
                    new { id = createdUser.UserId },
                    new ApiResponseSuccess<UserRolePermissionsModel>
                    {
                        StatusCode = StatusCodes.Status201Created,
                        Success = true,
                        Message = "Tạo người dùng thành công.",
                        Data = createdUser
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = ex.Message,
                    Error = "Lỗi khi tạo người dùng."
                });
            }
            catch (Exception ex)
            {
                
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Success = false,
                        Message = ex.Message,
                        Error = "Lỗi trong quá trình xử lý."
                    }
                );
            }
        }

        //G userCode by id
        [HttpGet("user-code/{userCode}")]
        public async Task<IActionResult> GetUserRolePermissionsByAsync(string userCode)
        {
            if (string.IsNullOrEmpty(userCode))
            {
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "User code không được để trống.",
                    Data = null
                });
            }
            try
            {
                var user = await _userService.GetUserRolePermissionsByIdAsync(userCode);
                if (user == null)
                {
                    return NotFound(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Success = false,
                        Message = "Người dùng không tồn tại.",
                        Data = null
                    });
                }
                // Loại bỏ password trước khi trả về
                user.Password = null;
                return Ok(new ApiResponseSuccess<UserRolePermissionsModel>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "Lấy người dùng thành công.",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message ,
                    Error = "Đã xảy ra lỗi trong quá trình xử lý."
                });
            }
        }

        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission([FromBody] PermissionModel model)
        {
            if (model == null)
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });

            try
            {
                var createdPermission = await _userService.CreatePermissionAsync(model);
                return CreatedAtAction(
                    nameof(GetAllPermissions),
                    new { id = createdPermission.PermissionId },
                    new ApiResponseSuccess<PermissionModel>
                    {
                        StatusCode = StatusCodes.Status201Created,
                        Success = true,
                        Message = "Tạo permission thành công",
                        Data = createdPermission
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = ex.Message,
                    Error = "Lỗi khi tạo permission"
                });
            }
        }

        [HttpPost("roles/{roleName}/permissions")]

        public async Task<IActionResult> AssignPermissionsToRole(
            string roleName,
            [FromBody] List<string> permissionCodes)
        {
            if (string.IsNullOrEmpty(roleName) || permissionCodes == null)
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });

            try
            {
                await _userService.AssignPermissionsToRoleAsync(roleName, permissionCodes);
                return Ok(new ApiResponseSuccess<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Success = true,
                    Message = "Gán permissions cho role thành công",
                    Data = roleName
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "Lỗi khi gán permissions",
                    Error = ex.Message
                });
            }
        }

        //[HttpPut("update/{id}")]
        //public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRolePermissionsModel model)
        //{
        //    if (model == null || id != model.UserId)
        //    {
        //        return BadRequest(new ApiResponseError
        //        {
        //            StatusCode = StatusCodes.Status400BadRequest,
        //            Success = false,
        //            Message = "Dữ liệu không hợp lệ hoặc ID không khớp.",
        //            Data = null
        //        });
        //    }

        //    try
        //    {
        //        // Validation cơ bản
        //        if (string.IsNullOrEmpty(model.UserName) ||
        //            string.IsNullOrEmpty(model.RoleName))
        //        {
        //            return BadRequest(new ApiResponseError
        //            {
        //                StatusCode = StatusCodes.Status400BadRequest,
        //                Success = false,
        //                Message = "Username và Role không được để trống.",
        //                Data = null
        //            });
        //        }

        //        var updatedUser = await _userService.UpdateUserWithRoleAndPermissionsAsync(model);

        //        // Loại bỏ password trước khi trả về
        //        updatedUser.Password = null;

        //        return Ok(new ApiResponseSuccess<UserRolePermissionsModel>
        //        {
        //            StatusCode = StatusCodes.Status200OK,
        //            Success = true,
        //            Message = "Cập nhật người dùng thành công.",
        //            Data = updatedUser
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {

        //        return BadRequest(new ApiResponseError
        //        {
        //            StatusCode = StatusCodes.Status400BadRequest,
        //            Success = false,
        //            Message = "Lỗi khi cập nhật người dùng.",
        //            Error = ex.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(
        //            StatusCodes.Status500InternalServerError,
        //            new ApiResponseError
        //            {
        //                StatusCode = StatusCodes.Status500InternalServerError,
        //                Success = false,
        //                Message = "Đã xảy ra lỗi trong quá trình xử lý.",
        //                Error = "Internal Server Error"
        //            }
        //        );
        //    }
        //}
    }
}
