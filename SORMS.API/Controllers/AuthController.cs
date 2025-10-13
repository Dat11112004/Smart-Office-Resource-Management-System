using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SORMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập và nhận JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
                return Unauthorized("Sai tên đăng nhập Email hoặc mật khẩu.");

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RegisterAsync(registerDto);
            if (!success)
                return Conflict("Tên hoặc Email đăng nhập đã tồn tại.");

            return Ok("Đăng ký thành công.");
        }

        /// <summary>
        /// Lấy thông tin người dùng theo username
        /// </summary>
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            return Ok(user);
        }

        // ================= Forgot Password Flow =================

        /// <summary>
        /// Gửi OTP về email để reset mật khẩu
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.SendOtpAsync(dto.Email);
            if (!success)
                return NotFound("Email không tồn tại trong hệ thống.");

            return Ok("OTP đã được gửi về email.");
        }

        /// <summary>
        /// Xác minh OTP
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValid = await _authService.VerifyOtpAsync(dto.Email, dto.Otp);
            if (!isValid)
                return BadRequest("OTP không hợp lệ hoặc đã hết hạn.");

            return Ok("Xác minh OTP thành công.");
        }

        /// <summary>
        /// Reset mật khẩu bằng OTP
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.ResetPasswordAsync(dto.Email, dto.Otp, dto.NewPassword);
            if (!success)
                return BadRequest("Không thể reset mật khẩu. OTP sai hoặc đã hết hạn.");

            return Ok("Đặt lại mật khẩu thành công.");
        }
    }
}
