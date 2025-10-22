using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog.Core;
using SORMS.API.Configs; // namespace chứa JwtConfig
using SORMS.API.Data;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using SORMS.API.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SORMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SormsDbContext _context;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
       

        public AuthService(SormsDbContext context, IOptions<JwtConfig> jwtOptions, IEmailService emailService, IConfiguration configuration, ILogger<AuthService>logger)
        {
            _context = context;
            _jwtConfig = jwtOptions.Value;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;

        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
                var user = await _context.Users
               .Include(u => u.Role)
               .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);
            return token;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == registerDto.Username || u.Email ==registerDto.Email);

            if (existingUser)
                return null;

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = HashPassword(registerDto.Password),
                RoleId = registerDto.RoleId,
                IsActive = true,
                Email = registerDto.Email // 🔹 nhớ thêm Email khi đăng ký
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Tự động tạo token sau khi đăng ký thành công
            var token = GenerateJwtToken(user);
            return token;
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

        // =================== FORGOT PASSWORD ===================

        // 1. Gửi OTP qua Email
        public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false
            ;

            var otp = new Random().Next(100000, 999999).ToString(); // 6 digits
            user.ResetOtp = otp;
            user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _context.SaveChangesAsync();

            // gửi email
            await _emailService.SendEmailAsync(email, "Mã OTP khôi phục mật khẩu", $"Mã OTP của bạn là: {otp}");

            return true;
        }

        // 2. Xác minh OTP
        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            if (user.ResetOtp != otp || user.ResetOtpExpiry < DateTime.UtcNow)
                return false;

            return true;
        }

        // 3. Đặt lại mật khẩu bằng OTP
        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            if (user.ResetOtp != otp || user.ResetOtpExpiry < DateTime.UtcNow)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            user.ResetOtp = null;
            user.ResetOtpExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        // =================== JWT & Helpers ===================

        public string GenerateJwtToken(User user)
        {
            try
            {
                // 1️⃣ Xác định vai trò dựa trên RoleId
                string roleName = user.RoleId switch
                {
                    1 => "Admin",
                    2 => "Staff",
                    3 => "Resident",
                    _ => "Resident" // mặc định nếu có lỗi hoặc null
                };

                // 2️⃣ Lấy secret key từ appsettings.json
                var key = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(key) || key.Length < 32)
                {
                    throw new InvalidOperationException(
                        "JWT Key must be configured in appsettings.json and be at least 32 characters long");
                }

                var keyBytes = Encoding.UTF8.GetBytes(key);
                var securityKey = new SymmetricSecurityKey(keyBytes);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
{
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),   // "sub"
                    new Claim("username", user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),     // name id
                    new Claim(ClaimTypes.Role, roleName),                         // schema role
                    new Claim("role", roleName)                                   // plain "role" for compatibility
};


                // 4️⃣ Đọc config JWT
                var issuer = _configuration["Jwt:Issuer"] ?? "SORMS.API";
                var audience = _configuration["Jwt:Audience"] ?? "SORMS.Client";
                var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

                // 5️⃣ Sinh JWT token
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: credentials
                );

                // 6️⃣ Chuyển token thành chuỗi
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"Token generated for user: {user.Username} with role {roleName}");

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating token: {ex.Message}");
                throw;
            }
        }


        //private string GenerateJwtToken(User user)
        //{
        //    var jwtSettings = _configuration.GetSection("JwtSettings");
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer64),
        //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //        new Claim(ClaimTypes.Role, user.RoleId.ToString())


        //    };

        //    var token = new JwtSecurityToken(
        //        issuer: jwtSettings["Issuer"],
        //        audience: jwtSettings["Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}


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
