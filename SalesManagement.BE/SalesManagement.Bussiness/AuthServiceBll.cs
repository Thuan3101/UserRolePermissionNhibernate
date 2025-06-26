using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SalesManagement.Common.Model;
using SalesManagement.Common.Supports;
using SalesManagement.Entities.Data;
using SalesManagement.Nhibernate;
using BCryptNet = BCrypt.Net.BCrypt;



namespace SalesManagement.Bussiness
{
    public class AuthServiceBll
    {
        private readonly ILogger<AuthServiceBll> _logger;
        private readonly IMapper _mapper;
        private readonly SessionManager _sessionManager;
        private readonly JwtToken _jwtToken;
        private const int MAX_FAILED_ATTEMPTS = 10;
        private const string STATUS_ACTIVE = "ACTIVE";
        private const string STATUS_STOP = "STOP";

        public AuthServiceBll(
            ILogger<AuthServiceBll> logger, 
            IMapper mapper, 
            SessionManager sessionManager,
            JwtToken jwtToken)
        {
            _logger = logger;
            _mapper = mapper;
            _sessionManager = sessionManager;
            _jwtToken = jwtToken;
        }
        //if (!BCryptNet.Verify(model.Password, user.Password))
        //{
        //    return new BadRequestObjectResult(new { message = "Mật khẩu không đúng" });
        //}
        public async Task<IActionResult> SignIn(SignInModel model)
        {
            try
            {
                User user;
                IList<string> roleNames;
                var userName = model.UserName.ToUpper();
                using (var session = SessionManager.NewIndependentSession)
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        User userAlias = null;
                        UserRole userRoleAlias = null;
                        Role roleAlias = null;

                        user = await session.QueryOver(() => userAlias)
                            .Left.JoinAlias(() => userAlias.UserRoles, () => userRoleAlias)
                            .Left.JoinAlias(() => userRoleAlias.Role, () => roleAlias)
                            .Where(() => userAlias.UserName == model.UserName)
                            .SingleOrDefaultAsync();

                        if (user == null)
                        {
                            throw new Exception("Tài khoản không tồn tại.");   
                        }

                        // Kiểm tra trạng thái tài khoản
                        if (user.Status == STATUS_STOP)
                        {
                            throw new Exception("Tài khoản đã bị khóa. Vui lòng liên hệ admin để mở khóa.");
                        }

                        if (user.Status != STATUS_ACTIVE)
                        {
                            throw new Exception($"Tài khoản không hoạt động: {user.Status}");
                        }

                        // Kiểm tra mật khẩu
                        if (!BCryptNet.Verify(model.Password, user.Password))
                        //if (user.Password != model.Password)
                        {
                            // Tăng số lần đăng nhập sai
                            user.FailedLoginAttempts++;
                            user.LastFailedLoginAttempt = DateTime.UtcNow;

                            // Kiểm tra nếu đã đạt giới hạn
                            if (user.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                            {
                                user.Status = STATUS_STOP;
                                await session.UpdateAsync(user);
                                await transaction.CommitAsync();

                                return new BadRequestObjectResult(new
                                {
                                    message = "Tài khoản đã bị khóa do đăng nhập sai nhiều lần. Vui lòng liên hệ admin."
                                });
                            }

                            await session.UpdateAsync(user);
                            await transaction.CommitAsync();

                            return new BadRequestObjectResult(new
                            {
                                message = $"Mật khẩu không đúng. Còn {MAX_FAILED_ATTEMPTS - user.FailedLoginAttempts} lần thử"
                            });
                        }

                        // Đăng nhập thành công - Reset số lần đăng nhập sai
                        user.FailedLoginAttempts = 0;
                        user.LastFailedLoginAttempt = null;
                        await session.UpdateAsync(user);

                        // Load roles before closing session
                        roleNames = await session.Query<UserRole>()
                            .Where(ur => ur.User.UserId == user.UserId)
                            .Select(ur => ur.Role.RoleName)
                            .ToListAsync();

                        // Create a detached copy of the user
                        var detachedUser = new User
                        {
                            UserId = user.UserId,
                            UserName = user.UserName,
                            Email = user.Email,
                            Status = user.Status
                        };

                        await transaction.CommitAsync();
                        user = detachedUser;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                var tokenModel = await _jwtToken.GenerateToken(user);
                return new OkObjectResult(tokenModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign in: {Message}", ex.Message);
                throw new Exception($"Đăng nhập thất bại: {ex.Message}");
            }
        }


        public async Task<IActionResult> SignOut(string userId)
        {
            try
            {
                if (!int.TryParse(userId, out int userIdInt))
                {
                    return new BadRequestObjectResult(new { message = "UserId không hợp lệ" });
                }

                using var session = SessionManager.CurrentSession;
                var refreshTokens = await session.Query<RefreshToken>()
                    .Where(rt => rt.UserId == userIdInt && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in refreshTokens)
                {
                    token.IsRevoked = true;
                    await session.UpdateAsync(token);
                }

                await session.FlushAsync();
                throw new Exception ("Đăng xuất thành công");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during sign out: {Message}", ex.Message);
                // Trả về lỗi nếu có vấn đề trong quá trình đăng xuất
                throw new Exception($"Đăng xuất thất bại: {ex.Message}");
            }
           
        }

        public async Task<AuthResult> SignUp(SignUpModel model)
        {
            try
            {
                using var session = SessionManager.NewIndependentSession;
                using var transaction = session.BeginTransaction();
                try
                {
                    var existingUserByEmail = await session.Query<User>()
                        .FirstOrDefaultAsync(x => x.Email == model.Email);

                    if (existingUserByEmail != null)
                    {
                        return AuthResult.Failed(new AuthError
                        {
                            Code = "EmailExists",
                            Description = "Email đã tồn tại."
                        });
                    }

                    var existingUserByUserName = await session.Query<User>()
                        .FirstOrDefaultAsync(x => x.UserName == model.UserName);

                    if (existingUserByUserName != null)
                    {
                        return AuthResult.Failed(new AuthError
                        {
                            Code = "UserNameExists",
                            Description = "UserName đã tồn tại."
                        });
                    }

                    var hashedPassword = BCryptNet.HashPassword(model.Password, workFactor: 12);
                    var userCode = await GenerateUserCodeAsync();
                    var user = new User
                    {
                        UserCode = userCode,
                        UserName = model.UserName.ToUpper(),
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Gender = model.Gender,
                        Password = hashedPassword,
                        Status = STATUS_ACTIVE,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    await session.SaveAsync(user);

                    // Tìm hoặc tạo vai trò 'User'
                    var customerRole = await session.Query<Role>()
                        .FirstOrDefaultAsync(r => r.RoleName == "User");

                    if (customerRole == null)
                    {
                        customerRole = new Role { RoleName = "User" };
                        await session.SaveAsync(customerRole);
                    }

                    // Gán vai trò cho người dùng
                    var userRole = new UserRole
                    {
                        User = user,
                        Role = customerRole
                    };

                    await session.SaveAsync(userRole);
                    await transaction.CommitAsync();

                    return AuthResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error during sign up: {Message}. Inner exception: {InnerMessage}",
                    ex.Message, innerExceptionMessage);
                return AuthResult.Failed(new AuthError
                {
                    Code = "SignUpFailed",
                    Description = $"An error occurred: {ex.Message}. Inner exception: {innerExceptionMessage}"
                });
            }
        }
        public async Task<IActionResult> ResetAccountStatus(string userCode)
        {
            try
            {
                using var session = SessionManager.CurrentSession;
                using var transaction = session.BeginTransaction();

                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.UserCode == userCode);

                if (user == null)
                {
                    return new NotFoundObjectResult(new { message = "Không tìm thấy tài khoản" });
                }

                user.Status = STATUS_ACTIVE;
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginAttempt = null;

                await session.UpdateAsync(user);
                await transaction.CommitAsync();

                return new OkObjectResult(new { message = "Đã mở khóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting account status: {Message}", ex.Message);
                return new BadRequestObjectResult(new { message = "Không thể mở khóa tài khoản" });
            }
        }

        //Generate UserCode 
        public async Task<string> GenerateUserCodeAsync(string role = "USER")
        {
            using var session = SessionManager.CurrentSession;

            // Standardize role prefix
            var prefix = role.ToUpper();

            // Get last user with same role prefix
            var lastUser = await session.Query<User>()
                .Where(u => u.UserCode.StartsWith(prefix))
                .OrderByDescending(u => u.UserCode)
                .FirstOrDefaultAsync();

            // Extract number from last code or start at 1
            int nextNumber;
            if (lastUser != null && lastUser.UserCode.Length > prefix.Length)
            {
                string currentNumber = lastUser.UserCode.Substring(prefix.Length);
                int.TryParse(currentNumber, out int currentSeq);
                nextNumber = currentSeq + 1;
            }
            else
            {
                nextNumber = 1;
            }

            // Format as PREFIX0001, PREFIX0002, etc.
            return $"{prefix}{nextNumber:D4}";
        }
        public async Task<User?> FindByEmailAsync(string email)
        {
            using var session = SessionManager.CurrentSession;
            return await session.Query<User>()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var session = SessionManager.CurrentSession;
                await session.UpdateAsync(user);
                await session.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Message}", ex.Message);
                throw new Exception($"Cập nhật người dùng thất bại: {ex.Message}");
            }
        }

        //refresh token

    }
}