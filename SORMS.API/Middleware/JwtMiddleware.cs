//namespace SORMS.API.Middleware
//{
//    public class JwtMiddleware
//    {
//        private readonly RequestDelegate _next;

//        public JwtMiddleware(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task Invoke(HttpContext context, IJwtService jwtService)
//        {
//            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

//            if (token != null && jwtService.ValidateToken(token, out var userId))
//            {
//                // Gán thông tin người dùng vào context nếu cần
//                context.Items["UserId"] = userId;
//            }

//            await _next(context);
//        }
//    }

//}
