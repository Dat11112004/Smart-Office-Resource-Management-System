using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SORMS.API.Configs; // namespace chứa JwtConfig
using SORMS.API.Data;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using SORMS.API.Models;

namespace SORMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SormsDbContext _context;
        private readonly JwtConfig _jwtConfig;

        public AuthService(SormsDbContext context, IOptions<JwtConfig> jwtOptions)
        {
            _context = context;
            _jwtConfig = jwtOptions.Value;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);
            return token;
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == registerDto.Username);

            if (existingUser)
                return false;

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = HashPassword(registerDto.Password),
                RoleId = registerDto.RoleId,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role.Name,
                IsActive = user.IsActive
            };
        }

        // Helper: Tạo JWT
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("UserId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtConfig.ExpireHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
