using AutoMapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using SalesManagement.Common.Model;
using SalesManagement.Common.Response;
using SalesManagement.Entities.Data;
using SalesManagement.Nhibernate;
using System.Text;
namespace SalesManagement.Bussiness
{
    public class UserServiceBll
    {
        private readonly ILogger<UserServiceBll> _logger; private readonly IMapper _mapper; private readonly SessionManager _sessionManager;
        public UserServiceBll(ILogger<UserServiceBll> logger, IMapper mapper, SessionManager sessionManager)
        {
            _logger = logger;
            _mapper = mapper;
            _sessionManager = sessionManager;
        }

        public async Task<List<UserModel>> GetAllUsersAsync(int pageSize, int currentPage, string? search)
        {
            try
            {
                using var session = SessionManager.NewIndependentSession;

                if (session == null)
                {
                    throw new InvalidOperationException("Failed to create database session");
                }

                // Use User entity instead of UserModel for NHibernate query
                var query = session.QueryOver<User>();

                if (!string.IsNullOrEmpty(search))
                {
                    query.Where(
                        Restrictions.Disjunction()
                            .Add(Restrictions.Like("UserName", $"%{search}%"))
                            .Add(Restrictions.Like("FirstName", $"%{search}%"))
                            .Add(Restrictions.Like("LastName", $"%{search}%"))
                    );
                }

                // Get paginated results
                var users = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ListAsync();

                // Map to UserModel and create paginated response
                var userModels = _mapper.Map<List<UserModel>>(users ?? new List<User>());
                return userModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users.");
                throw;
            }
        }

        // Get all roles
        public async Task<List<RoleModel>> GetAllRolesAsync(int pageSize, int currentPage, string? search)
        {
            try
            {
                using var session = SessionManager.NewIndependentSession;

                if (session == null)
                {
                    throw new InvalidOperationException("Failed to create database session");
                }


                var query = session.QueryOver<Role>();

                if (!string.IsNullOrEmpty(search))
                {
                    query.Where(
                        Restrictions.Disjunction()
                            .Add(Restrictions.Like("RoleId", $"%{search}%"))
                            .Add(Restrictions.Like("RoleName", $"%{search}%"))
                            .Add(Restrictions.Like("Description", $"%{search}%"))
                    );
                }

                // Get paginated results
                var roles = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ListAsync();

                // Map to UserModel and create paginated response
                var roleModels = _mapper.Map<List<RoleModel>>(roles ?? new List<Role>());
                return roleModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all roles.");
                throw;
            }
        }

        //Get all permissions
        public async Task<List<PermissionModel>> GetAllPermissionsAsync(int pageSize, int currentPage, string? search)
        {
            try
            {
                using var session = SessionManager.NewIndependentSession;
                if (session == null)
                {
                    throw new InvalidOperationException("Failed to create database session");
                }
                var query = session.QueryOver<Permission>();
                if (!string.IsNullOrEmpty(search))
                {
                    query.Where(
                        Restrictions.Disjunction()
                            .Add(Restrictions.Like("PermissionId", $"%{search}%"))
                            .Add(Restrictions.Like("PermissionCode", $"%{search}%"))
                            .Add(Restrictions.Like("PermissionName", $"%{search}%"))
                            .Add(Restrictions.Like("Description", $"%{search}%"))
                    );
                }
                // Get paginated results
                var permissions = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ListAsync();
                // Map to UserModel and create paginated response
                var permissionModels = _mapper.Map<List<PermissionModel>>(permissions ?? new List<Permission>());
                return permissionModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all permissions.");
                throw;
            }
        }
        // Get all user role permissions
        public async Task<List<UserRolePermissionsModel>> GetAllUserRolePermissionsAsync(int pageSize, int currentPage, string? search)
        {
            try
            {
                using var session = SessionManager.NewIndependentSession;

                var whereClause = new StringBuilder();
                var parameters = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(search))
                {
                    whereClause.Append(@"
                        AND (
                            u.UserCode LIKE :Search
                            OR u.UserName LIKE :Search
                            OR u.FirstName LIKE :Search
                            OR u.LastName LIKE :Search
                            OR u.Gender LIKE :Search
                            OR u.Email LIKE :Search
                            OR u.Phone LIKE :Search
                            OR u.Status LIKE :Search
                            OR r.RoleName LIKE :Search
                            OR p.PermissionCode LIKE :Search
                        )");
                    parameters["Search"] = $"%{search}%";
                }

                // Query đếm tổng số bản ghi
                var countSql = $@"
                    SELECT COUNT(DISTINCT u.UserID)
                    FROM Users u
                    LEFT JOIN UserRoles ur ON u.UserID = ur.UserID
                    LEFT JOIN Roles r ON ur.RoleID = r.RoleID
                    WHERE 1=1 {whereClause}";

                var countQuery = session.CreateSQLQuery(countSql);
                if (!string.IsNullOrEmpty(search))
                {
                    countQuery.SetParameter("Search", parameters["Search"]);
                }
                var totalCount = await countQuery.UniqueResultAsync<int>();

                // Query lấy thông tin users với phân trang
                var userSql = $@"
                        SELECT 
                            CAST(u.UserID AS INT) as UserId,
                            u.UserCode,
                            u.UserName,
                            u.FirstName,
                            u.LastName,
                            u.Gender,
                            u.Email,
                            u.Birthdate as DateOfBirth,
                            u.Phone,
                            
                            u.Status,
                            u.CreatedAt,
                            u.UpdatedAt,
                            r.RoleName
                        FROM Users u
                        LEFT JOIN UserRoles ur ON u.UserID = ur.UserID
                        LEFT JOIN Roles r ON ur.RoleID = r.RoleID
                        WHERE 1=1 {whereClause}
                        ORDER BY u.UserID
                        OFFSET :Skip ROWS
                        FETCH NEXT :Take ROWS ONLY";

                var userQuery = session.CreateSQLQuery(userSql)
                    .SetParameter("Skip", (currentPage - 1) * pageSize)
                    .SetParameter("Take", pageSize);

                if (!string.IsNullOrEmpty(search))
                {
                    userQuery.SetParameter("Search", parameters["Search"]);
                }

                var users = await userQuery
                    .SetResultTransformer(Transformers.AliasToBean<UserRolePermissionsModel>())
                    .ListAsync<UserRolePermissionsModel>();

                // Explicitly cast the result to List<UserRolePermissionsModel>
                var userList = users?.ToList() ?? new List<UserRolePermissionsModel>();

                if (!userList.Any())
                    return new List<UserRolePermissionsModel>();

                // Query lấy permissions cho tất cả users
                var permissionsSql = @"
                    SELECT 
                        u.UserCode,
                        p.PermissionCode
                    FROM Users u
                    JOIN UserRoles ur ON u.UserID = ur.UserID
                    JOIN Roles r ON ur.RoleID = r.RoleID
                    JOIN RolePermissions rp ON r.RoleID = rp.RoleID
                    JOIN Permissions p ON rp.PermissionCode = p.PermissionCode
                    WHERE u.UserCode IN (:UserCodes)";

                var userCodes = userList.Select(u => u.UserCode).ToList();
                var permissionsResult = await session.CreateSQLQuery(permissionsSql)
                    .SetParameterList("UserCodes", userCodes)
                    .ListAsync<object[]>();

                // Map permissions vào từng user
                var permissionsMap = new Dictionary<string, List<string>>();
                foreach (var row in permissionsResult)
                {
                    var userCode = row[0]?.ToString();
                    var permission = row[1]?.ToString();

                    if (!string.IsNullOrEmpty(userCode) && !string.IsNullOrEmpty(permission))
                    {
                        if (!permissionsMap.ContainsKey(userCode))
                            permissionsMap[userCode] = new List<string>();

                        permissionsMap[userCode].Add(permission);
                    }
                }

                // Gán permissions vào users
                foreach (var user in userList)
                {
                    user.Permissions = permissionsMap.GetValueOrDefault(user.UserCode, new List<string>());
                }

                return userList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving user role permissions");
                throw;
            }
        }


        //Create a new user
        public async Task<UserRolePermissionsModel> CreateUserWithRoleAndPermissionsAsync(UserRolePermissionsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrEmpty(model.UserCode))
                throw new ArgumentException("UserCode không được để trống");

            if (string.IsNullOrEmpty(model.UserName))
                throw new ArgumentException("UserName không được để trống");

            if (string.IsNullOrEmpty(model.RoleName))
                throw new ArgumentException("RoleName không được để trống");

            try
            {
                using var session = SessionManager.NewIndependentSession;
                using var transaction = session.BeginTransaction();

                try
                {
                    // Kiểm tra UserCode đã tồn tại chưa
                    var checkExisting = await session.CreateSQLQuery("SELECT COUNT(1) FROM Users WHERE UserCode = :UserCode")
                        .SetParameter("UserCode", model.UserCode)
                        .UniqueResultAsync<int>();
                    if (checkExisting > 0)
                    {
                        throw new InvalidOperationException($"UserCode '{model.UserCode}' đã tồn tại");
                    }
                    //kiểm tra uername đã tồn tại chưa
                    var checkExistingUserName = await session.CreateSQLQuery("SELECT COUNT(1) FROM Users WHERE UserName = :UserName")
                        .SetParameter("UserName", model.UserName)
                        .UniqueResultAsync<int>();

                    if (checkExistingUserName > 0)
                    {
                        throw new InvalidOperationException($"UserName '{model.UserName}' đã tồn tại");
                    }
                    //kiểm tra email đã tồn tại chưa
                    var checkExistingEmail = await session.CreateSQLQuery("SELECT COUNT(1) FROM Users WHERE Email = :Email")
                        .SetParameter("Email", model.Email)
                        .UniqueResultAsync<int>();
                    if (checkExistingEmail > 0)
                    {
                        throw new InvalidOperationException($"Email '{model.Email}' đã tồn tại");
                    }

                    // 1. Insert User và lấy ID
                    var insertUserSql = @"
                        INSERT INTO Users (
                            UserCode, UserName, FirstName, LastName, 
                            Gender, Email, Birthdate, Phone, 
                             Status, CreatedAt, UpdatedAt
                        ) 
                        VALUES (
                            :UserCode, :UserName, :FirstName, :LastName,
                            :Gender, :Email, :Birthdate, :Phone,
                             :Status, :CreatedAt, :UpdatedAt
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    var currentDateTime = DateTime.Now;

                    var result = await session.CreateSQLQuery(insertUserSql)
                        .SetParameter("UserCode", model.UserCode)
                        .SetParameter("UserName", model.UserName)
                        .SetParameter("FirstName", model.FirstName)
                        .SetParameter("LastName", model.LastName)
                        .SetParameter("Gender", model.Gender)
                        .SetParameter("Email", model.Email)
                        .SetParameter("Birthdate", model.DateOfBirth.ToString("yyyy-MM-dd"))
                        .SetParameter("Phone", model.Phone)
                        
                        .SetParameter("Status", model.Status ?? "ACTIVE")
                        .SetParameter("CreatedAt", currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                        .SetParameter("UpdatedAt", currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                        .UniqueResultAsync();

                    var userId = Convert.ToInt32(result);

                    // 2. Get RoleId 
                    var getRoleIdSql = @"
                SELECT CAST(RoleID AS INT) 
                FROM Roles 
                WHERE RoleName = :RoleName";

                    var roleResult = await session.CreateSQLQuery(getRoleIdSql)
                        .SetParameter("RoleName", model.RoleName)
                        .UniqueResultAsync();

                    var roleId = Convert.ToInt32(roleResult);

                    if (roleId == 0)
                    {
                        throw new InvalidOperationException($"Role '{model.RoleName}' không tồn tại");
                    }

                    // 3. Insert UserRole
                    var insertUserRoleSql = @"
                INSERT INTO UserRoles (UserID, RoleID) 
                VALUES (:UserId, :RoleId)";

                    await session.CreateSQLQuery(insertUserRoleSql)
                        .SetParameter("UserId", userId)
                        .SetParameter("RoleId", roleId)
                        .ExecuteUpdateAsync();

                    await transaction.CommitAsync();

                    //// 4. Return created user
                    //return await GetUserRolePermissionsByIdAsync(model.UserCode);
                }

                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo user với role: {UserCode}", model.UserCode);
                throw;
            }
            // 💡 Lưu ý: Đoạn này tách khỏi using để tránh dùng session đã dispose
            var createdUser = await GetUserRolePermissionsByIdAsync(model.UserCode);
            return createdUser;
        }

        public async Task<UserRolePermissionsModel> GetUserRolePermissionsByIdAsync(string userCode)
        {
            if (string.IsNullOrEmpty(userCode))
                throw new ArgumentException("UserCode không được để trống");

            using var session = SessionManager.NewIndependentSession;

            // Query lấy thông tin user
            var userSql = @"
                SELECT 
                    CAST(u.UserID AS INT) as UserId,
                    u.UserCode,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.Gender,
                    u.Email,
                    u.Birthdate as DateOfBirth,
                    u.Phone,
                    
                    u.Status,
                    u.CreatedAt,
                    u.UpdatedAt,
                    r.RoleName
                FROM Users u
                LEFT JOIN UserRoles ur ON u.UserID = ur.UserID
                LEFT JOIN Roles r ON ur.RoleID = r.RoleID
                WHERE u.UserCode = :UserCode";

                        // Query lấy danh sách permissions
                        var permissionsSql = @"
                SELECT DISTINCT p.PermissionCode
                FROM Users u
                JOIN UserRoles ur ON u.UserID = ur.UserID
                JOIN Roles r ON ur.RoleID = r.RoleID
                JOIN RolePermissions rp ON r.RoleID = rp.RoleID
                JOIN Permissions p ON rp.PermissionCode = p.PermissionCode
                WHERE u.UserCode = :UserCode";

            try
            {
                // Lấy thông tin user
                var userInfo = await session.CreateSQLQuery(userSql)
                    .SetParameter("UserCode", userCode)
                    .SetResultTransformer(Transformers.AliasToBean<UserRolePermissionsModel>())
                    .UniqueResultAsync<UserRolePermissionsModel>();

                if (userInfo == null)
                    throw new InvalidOperationException($"Không tìm thấy user với UserCode: {userCode}");

                // Lấy danh sách permissions
                var permissions = await session.CreateSQLQuery(permissionsSql)
                    .SetParameter("UserCode", userCode)
                    .ListAsync<string>();

                // Gán permissions vào model
                userInfo.Permissions = permissions?.ToList() ?? new List<string>();

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user: {UserCode}", userCode);
                throw;
            }
        }

        public async Task<PermissionModel> CreatePermissionAsync(PermissionModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(model.PermissionCode))
                throw new ArgumentException("PermissionCode không được để trống");

            if (string.IsNullOrEmpty(model.PermissionName))
                throw new ArgumentException("PermissionName không được để trống");

            try
            {
                using var session = SessionManager.NewIndependentSession;
                using var transaction = session.BeginTransaction();

                try
                {
                    // Kiểm tra PermissionName đã tồn tại chưa
                    var checkExisting = await session.CreateSQLQuery(
                        "SELECT COUNT(1) FROM Permissions WHERE PermissionCode = :PermissionCode")
                        .SetParameter("PermissionCode", model.PermissionCode)
                        .UniqueResultAsync<int>();

                    if (checkExisting > 0)
                        throw new InvalidOperationException($"Permission '{model.PermissionCode}' đã tồn tại");

                    // Insert Permission
                    var insertSql = @"
                INSERT INTO Permissions (PermissionCode, PermissionName, Description)
                VALUES (:PermissionCode, :PermissionName, :Description);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    var result = await session.CreateSQLQuery(insertSql)
                        .SetParameter("PermissionCode", model.PermissionCode)
                        .SetParameter("PermissionName", model.PermissionName)
                        .SetParameter("Description", model.Description)
                        .UniqueResultAsync();

                    var permissionId = Convert.ToInt32(result);
                    model.PermissionId = permissionId;

                    await transaction.CommitAsync();
                    return model;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo permission: {PermissionName}", model.PermissionCode);
                throw;
            }
        }

        public async Task AssignPermissionsToRoleAsync(string roleName, List<string> permissionCodes)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentException("RoleName không được để trống");

            if (permissionCodes == null || !permissionCodes.Any())
                throw new ArgumentException("Danh sách permission không được trống");

            try
            {
                using var session = SessionManager.NewIndependentSession;
                using var transaction = session.BeginTransaction();

                try
                {
                    // Lấy RoleId
                    var getRoleIdSql = @"
                SELECT CAST(RoleID AS INT)
                FROM Roles 
                WHERE RoleName = :RoleName";

                    var roleId = await session.CreateSQLQuery(getRoleIdSql)
                        .SetParameter("RoleName", roleName)
                        .UniqueResultAsync<int>();

                    if (roleId == 0)
                        throw new InvalidOperationException($"Role '{roleName}' không tồn tại");

                    // Kiểm tra các PermissionCodes có tồn tại
                    var validatePermissionsSql = @"
                SELECT PermissionCode
                FROM Permissions
                WHERE PermissionCode IN (:PermissionCodes)";

                    var validPermissionCodes = await session.CreateSQLQuery(validatePermissionsSql)
                        .SetParameterList("PermissionCodes", permissionCodes)
                        .ListAsync<string>();

                    if (validPermissionCodes.Count != permissionCodes.Count)
                    {
                        var invalidCodes = permissionCodes.Except(validPermissionCodes).ToList();
                        throw new InvalidOperationException($"Các permission code không tồn tại: {string.Join(", ", invalidCodes)}");
                    }

                    // Lấy các permissions hiện tại của role
                    var getCurrentPermissionsSql = @"
                SELECT PermissionCode
                FROM RolePermissions
                WHERE RoleID = :RoleId";

                    var currentPermissionCodes = await session.CreateSQLQuery(getCurrentPermissionsSql)
                        .SetParameter("RoleId", roleId)
                        .ListAsync<string>();

                    // Chỉ thêm các permissions chưa có
                    var permissionsToAdd = permissionCodes
                        .Except(currentPermissionCodes)
                        .ToList();

                    if (permissionsToAdd.Any())
                    {
                        // Thêm role permissions mới
                        var insertRolePermissionSql = @"
                    INSERT INTO RolePermissions (RoleID, PermissionCode)
                    VALUES (:RoleId, :PermissionCode)";

                        foreach (var permissionCode in permissionsToAdd)
                        {
                            await session.CreateSQLQuery(insertRolePermissionSql)
                                .SetParameter("RoleId", roleId)
                                .SetParameter("PermissionCode", permissionCode)
                                .ExecuteUpdateAsync();
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán permissions cho role: {RoleName}", roleName);
                throw;
            }
        }

    }
}

