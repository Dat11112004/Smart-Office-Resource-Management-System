using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> GetUserByUsernameAsync(string username);
    }

}
