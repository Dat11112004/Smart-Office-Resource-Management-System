using Microsoft.AspNetCore.Http;
using SORMS.API.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SORMS.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService)
        {
            // Lấy token từ Header Authorization
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Validate token → nếu hợp lệ thì gán UserId vào context
                    if (jwtService.ValidateToken(token, out var userId))
                    {
                        // Lưu thông tin user vào HttpContext để controller hoặc service khác có thể dùng
                        context.Items["UserId"] = userId;
                    }
                }
                catch
                {
                    // Nếu token lỗi hoặc hết hạn, middleware không chặn — để controller xử lý Unauthorized
                }
            }

            await _next(context);
        }
    }
}
