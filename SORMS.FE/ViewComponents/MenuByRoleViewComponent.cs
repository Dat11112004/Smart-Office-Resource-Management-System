using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace SORMS.FE.ViewComponents
{
    public class MenuByRoleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int[] allowedRoles, string menuId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            int currentRoleId = 0;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    
                    // Lấy role name từ claim
                    var roleName = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                    
                    // Convert role name sang role ID
                    currentRoleId = roleName switch
                    {
                        "Admin" => 1,
                        "Staff" => 2,
                        "Resident" => 3,
                        _ => 0
                    };

                    Console.WriteLine($"[MenuByRole] Token found - Role: {roleName}, RoleId: {currentRoleId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MenuByRole] Error parsing token: {ex.Message}");
                    // Token invalid, không có quyền
                }
            }
            else
            {
                Console.WriteLine("[MenuByRole] No token found in session");
            }

            // Kiểm tra quyền
            bool hasAccess = allowedRoles.Contains(currentRoleId);
            Console.WriteLine($"[MenuByRole] Menu: {menuId}, Allowed: [{string.Join(",", allowedRoles)}], Current: {currentRoleId}, Access: {hasAccess}");
            
            if (!hasAccess)
            {
                return Content(string.Empty); // Không hiển thị menu
            }

            // Trả về partial view tương ứng với menuId
            return View(menuId);
        }
    }
}
