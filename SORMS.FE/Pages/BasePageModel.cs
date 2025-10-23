using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SORMS.FE.Models;

namespace SORMS.FE.Pages
{
    public class BasePageModel : PageModel
    {
        public UserInfo CurrentUser { get; set; } = new UserInfo();

        protected void LoadCurrentUser()
        {
            var token = HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token))
            {
                CurrentUser.IsAuthenticated = false;
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                CurrentUser.IsAuthenticated = true;
                
                // Thử nhiều claim types
                CurrentUser.Username = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value 
                                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value 
                                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                                    ?? "Unknown";
                
                CurrentUser.Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "";
                CurrentUser.Role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
                
                // Convert role name to role ID
                CurrentUser.RoleId = CurrentUser.Role switch
                {
                    "Admin" => 1,
                    "Staff" => 2,
                    "Resident" => 3,
                    _ => 0
                };

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    CurrentUser.Id = userId;
                }
            }
            catch
            {
                CurrentUser.IsAuthenticated = false;
            }
        }

        protected bool IsInRole(params string[] roles)
        {
            return roles.Contains(CurrentUser.Role, StringComparer.OrdinalIgnoreCase);
        }

        protected bool HasRoleId(params int[] roleIds)
        {
            return roleIds.Contains(CurrentUser.RoleId);
        }
    }
}
