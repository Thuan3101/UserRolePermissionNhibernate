using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Linq;
using SalesManagement.Common.Model;
using SalesManagement.Entities.Data;
using SalesManagement.Nhibernate;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Supports
{
    public class JwtToken
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public JwtToken(IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }

        public async Task<TokenModel> GenerateToken(User user)
        {
            if (string.IsNullOrEmpty(_appSettings.SecretKey))
                throw new InvalidOperationException("Secret key is not configured");

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            // Create new session for role retrieval
            List<string> roleNames;
            using (var session = SessionManager.NewIndependentSession)
            {
                roleNames = await session.Query<UserRole>()
                    .Where(ur => ur.User.UserId == user.UserId)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
            };

            claims.AddRange(roleNames.Select(roleName => new Claim(ClaimTypes.Role, HashRoleName(roleName))));

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKeyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            // Create new session for refresh token save
            using (var session = SessionManager.NewIndependentSession)
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var refreshTokenEntity = new RefreshToken
                    {
                        JwtId = token.Id,
                        UserId = user.UserId,
                        User = user,  // Add this line
                        Token = refreshToken,
                        IsUsed = false,
                        IsRevoked = false,
                        IssuedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddHours(8)
                    };

                    await session.SaveAsync(refreshTokenEntity);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }
        private string HashRoleName(string roleName)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(roleName);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}