using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Cfg;
using SalesManagement.Api.Controllers.Parameters;
using SalesManagement.Bussiness;
using SalesManagement.Common.Model;
using SalesManagement.Common.Response;
using SalesManagement.Common.Supports;
using SalesManagement.Entities.Data;
using SalesManagement.Nhibernate;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Web.Http.Results;
using AppSettings = SalesManagement.Common.Model.AppSettings;
using OkResult = System.Web.Http.Results.OkResult;

namespace SalesManagement.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthServiceBll _authService;
        private readonly AppSettings _appSettings;
       
        private readonly JwtToken _jwtToken;

        public AuthController(
            AuthParameters authParameters,
            IOptions<AppSettings> appSettings,
            
            JwtToken jwtToken)
        {
            _authService = authParameters.AuthService;
            _appSettings = appSettings.Value;
            
            _jwtToken = jwtToken;
        }
        // POST: api/Auth/SignUp
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            try
            {
                var signUpResult = await _authService.SignUp(model);

                // Adjusted logic to handle AuthResult type
                if (signUpResult != null && !signUpResult.Succeeded)
                {
                    return BadRequest(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Success = false,
                        Message = "No Sign up ",
                        Data = null
                    });
                }

                if (signUpResult != null && signUpResult.Succeeded)
                {
                    return Ok(new ApiResponseSuccess<TokenModel>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Success = true,
                        Message = "Sign up successful",
                    });
                }

                // Handle unexpected result types
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "Unexpected result type from SignUp service.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "An error occurred while processing the sign-up request.",
                    Error = ex.Message
                });
            }
        }

        /// POST: api/Auth/SignIn

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            try
            {
                var signInResult = await _authService.SignIn(model);
                if (signInResult is BadRequestObjectResult badRequest)
                {
                    return BadRequest(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Success = false,
                        Message = badRequest.Value.ToString(),
                        Data = null
                    });
                }
                if (signInResult is OkObjectResult okResult)
                {
                    return Ok(new ApiResponseSuccess<TokenModel>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Success = true,
                        Message = "Sign in successful",
                        Data = okResult.Value as TokenModel
                    });
                }

                // Handle unexpected result types
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "Unexpected result type from SignIn service.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while processing the sign-in request."
                });
            }
        }

        [HttpPost("reset-account")]
        public async Task<IActionResult> ResetAccountStatus(string userCode)
        {
            try
            {
                var resetAccountStatusResult = await _authService.ResetAccountStatus(userCode);

                if (resetAccountStatusResult is OkObjectResult okResult)
                {
                    return Ok(new ApiResponseSuccess<TokenModel>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Success = true,
                        Message = "Reset successful",
                        Data = okResult.Value as TokenModel 
                    });
                }

                // Handle unexpected result types
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "Unexpected result type from ResetAccountStatus service.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "An error occurred while processing the reset account status request."
                });
            }
        }

        //
        [HttpPost("renew-token")]
        [SwaggerOperation(
        Summary = "Gia hạn token",
        Description = "Gia hạn token khi token hết hạn")]
        [SwaggerResponse(StatusCodes.Status200OK, "Gia hạn token thành công.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token không hợp lệ")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            if (string.IsNullOrEmpty(model.AccessToken) || string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new ApiResponseError
                {
                    Success = false,
                    Message = "Access token và Refresh token không được để trống"
                });
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };

            try
            {
                // Validate token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(
                    model.AccessToken,
                    tokenValidateParam,
                    out var validatedToken);

                // Validate algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var alg = jwtSecurityToken.Header.Alg;
                    if (!alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Algorithm không hợp lệ"
                        });
                    }
                }

                // Check token expiration
                var expClaim = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
                if (expClaim == null)
                {
                    return BadRequest(new ApiResponseError
                    {
                        Success = false,
                        Message = "Token không chứa thông tin hết hạn"
                    });
                }

                var utcExpireDate = long.Parse(expClaim.Value);
                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);

                if (expireDate > DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponseError
                    {
                        Success = false,
                        Message = "Token chưa hết hạn"
                    });
                }

                using (var session = SessionManager.CurrentSession)
                {
                    // Validate refresh token
                    var storedToken = await session.QueryOver<RefreshToken>()
                        .Where(x => x.Token == model.RefreshToken)
                        .SingleOrDefaultAsync();

                    if (storedToken == null)
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Refresh token không tồn tại"
                        });
                    }

                    if (storedToken.IsUsed || storedToken.IsRevoked)
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Refresh token đã được sử dụng hoặc đã bị thu hồi"
                        });
                    }

                    if (storedToken.ExpiredAt < DateTime.UtcNow)
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Refresh token đã hết hạn"
                        });
                    }

                    // Validate JwtId
                    var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                    if (storedToken.JwtId != jti)
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Token không khớp"
                        });
                    }

                    // Update old refresh token
                    storedToken.IsUsed = true;
                    storedToken.IsRevoked = true;
                    await session.UpdateAsync(storedToken);

                    // Get user
                    var user = await session.GetAsync<User>(storedToken.UserId);
                    if (user == null)
                    {
                        return BadRequest(new ApiResponseError
                        {
                            Success = false,
                            Message = "Không tìm thấy người dùng"
                        });
                    }

                    var newToken = await _jwtToken.GenerateToken(user);
                    await session.FlushAsync();

                    return Ok(new ApiResponseSuccess<TokenModel>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Success = true,
                        Message = "Token đã được gia hạn thành công",
                        Data = new TokenModel
                        {
                            AccessToken = newToken.AccessToken,
                            RefreshToken = newToken.RefreshToken
                        }
                    });
                }
            }
            catch (SecurityTokenException)
            {
                return BadRequest(new ApiResponseError
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Success = false,
                    Message = "Token không hợp lệ hoặc đã hết hạn"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = ex.Message,
                    Error = "Lỗi server khi gia hạn token"
                });
            }
        }
        // Phương thức để chuyển đổi UnixTime sang DateTime
        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {

            var dateTimeInterval = DateTimeOffset.FromUnixTimeSeconds(utcExpireDate).UtcDateTime;
            return dateTimeInterval;
        }

        // GET: api/Auth/SignOut
        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                // Get the user ID from the JWT token claims
                var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");
                if (userIdClaim == null)
                {
                    return BadRequest(new ApiResponseError
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Success = false,
                        Message = "User ID not found in token",
                        Data = null
                    });
                }

                var userId = userIdClaim.Value;
                var signOutResult = await _authService.SignOut(userId);

                if (signOutResult is OkResult)
                {
                    return Ok(new ApiResponseSuccess<object>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Success = true,
                        Message = "Sign out successful",
                        Data = null
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "Unexpected result type from SignOut service.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseError
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = "An error occurred while processing the sign-out request.",
                    Error = ex.Message
                });
            }
        }

    }
}
