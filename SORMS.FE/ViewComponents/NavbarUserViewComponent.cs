using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SORMS.FE.Models;

namespace SORMS.FE.ViewComponents
{
    public class NavbarUserViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            var userInfo = new UserInfo();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    userInfo.IsAuthenticated = true;
                    
                    // Thử các claim names khác nhau
                    userInfo.Username = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value 
                                     ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value 
                                     ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                                     ?? "Unknown";
                    
                    userInfo.Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "";
                    
                    // RoleId từ role name
                    var roleName = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
                    userInfo.RoleId = roleName switch
                    {
                        "Admin" => 1,
                        "Staff" => 2,
                        "Resident" => 3,
                        _ => 0
                    };
                }
                catch
                {
                    userInfo.IsAuthenticated = false;
                }
            }

            return View(userInfo);
        }
    }
}
