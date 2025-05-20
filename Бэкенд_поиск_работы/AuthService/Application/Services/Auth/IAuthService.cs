using AuthService.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Application.Services.Auth
{
    public interface IAuthService
    {
        public Task RegisterUser(RegisterDataDTO data);
        public Task<TokenPairDTO> LoginUser(LoginDataDTO data);
        public Task<TokenPairDTO> RefreshToken(string refreshToken, int userId);
        public Task<UserDataDTO> GetUserInfoFromToken(int id, string role);

    }
}
