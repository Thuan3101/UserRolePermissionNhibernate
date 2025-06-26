using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SalesManagement.Bussiness;
using SalesManagement.Common.Response;
using SalesManagement.Entities.Enum;
using SalesManagement.Nhibernate;

namespace SalesManagement.Api.Authorization
{

    public class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(string permission) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { permission };
            Order = int.MinValue;
        }
    }


    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly ILogger<PermissionFilter> _logger;

        public PermissionFilter(string permission, ILogger<PermissionFilter> logger)
        {
            _permission = permission;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                // 1. Kiểm tra token
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Unauthorized access attempt - no authentication");
                    context.Result = new JsonResult(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Success = false,
                        Message = "Unauthorized access attempt",
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                // 2. Lấy userId từ token
                var userId = context.HttpContext.User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No UserId found in token");
                    context.Result = new JsonResult(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Success = false,
                        Message = "Invalid token - UserId not found",
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                using var session = SessionManager.NewIndependentSession;
                using var transaction = session.BeginTransaction(); // Fix: Use synchronous BeginTransaction method

                try
                {
                    // 3. Kiểm tra quyền
                    var permissionCheckSql = @"
                        SELECT COUNT(1)
                        FROM Users u
                        JOIN UserRoles ur ON u.UserID = ur.UserID
                        JOIN Roles r ON ur.RoleID = r.RoleID
                        JOIN RolePermissions rp ON r.RoleID = rp.RoleID
                        JOIN Permissions p ON rp.PermissionCode = p.PermissionCode
                        WHERE u.UserId = :UserId 
                        AND u.Status = 'ACTIVE'
                        AND p.PermissionCode = :PermissionCode";

                    _logger.LogInformation($"Checking permission {_permission} for user {userId}");

                    var hasPermission = await session.CreateSQLQuery(permissionCheckSql)
                        .SetParameter("UserId", int.Parse(userId))
                        .SetParameter("PermissionCode", _permission)
                        .UniqueResultAsync<int>();

                    if (hasPermission == 0)
                    {
                        _logger.LogWarning($"User {userId} doesn't have permission {_permission}");
                        context.Result = new JsonResult(new ApiResponseError
                        {
                            StatusCode = StatusCodes.Status403Forbidden,
                            Success = false,
                            Message = $"Access denied. Required permission: {_permission}",
                        })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }

                    transaction.Commit(); // Fix: Use synchronous Commit method
                    _logger.LogInformation($"Access granted for user {userId} with permission {_permission}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Fix: Use synchronous Rollback method
                    throw new Exception("Error checking permissions", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in permission check");
                context.Result = new JsonResult(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "Internal server error during permission check"
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }

}