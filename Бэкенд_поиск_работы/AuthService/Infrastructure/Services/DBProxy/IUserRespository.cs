using AuthService.Application.DTO;
using AuthService.Infrastructure.DB.Models;

namespace AuthService.Infrastructure.Services.DBProxy
{
    public interface IUserRespository
    {
        public Task<UserDataDTO> GetUserByEmailAsync(string email);
        public Task<UserDataDTO> GetUserByIdAsync(int id);
        public Task<UserDataDTO> GetUserByLoginDataAsync(LoginDataDTO data);
        public Task CreateUserAsync(RegisterDataDTO data);
        public Task SaveChangesAsync();
        public Task ChangeUserRefreshTokenAsync(int id, string refresh);
    }
}
