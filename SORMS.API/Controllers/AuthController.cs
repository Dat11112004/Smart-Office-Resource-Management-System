using Microsoft.AspNetCore.Mvc;
using SORMS.API.DTOs;
using SORMS.API.Interfaces;

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
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");

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
                return Conflict("Tên đăng nhập đã tồn tại.");

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
    }
}
