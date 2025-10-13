using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> GetUserByUsernameAsync(string username);

        Task<bool> SendOtpAsync(string email);

        Task<bool> VerifyOtpAsync(string email, string otp);

        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);


    }

}
