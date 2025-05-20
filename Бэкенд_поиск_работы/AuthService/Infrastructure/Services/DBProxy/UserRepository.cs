using AuthService.Application.DTO;
using AuthService.Infrastructure.DB.DBContext;
using AuthService.Infrastructure.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Services.DBProxy
{
    public class UserRepository:IUserRespository
    {
        private readonly Context dbContext;
        public UserRepository(Context db)
        {
            dbContext = db;
        }
        public async Task<UserDataDTO> GetUserByEmailAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u=>u.Email==email);
            if (user == null) {
                return null;
            }
            return new UserDataDTO { Email = user.Email, Id=user.Id,Role=user.Role,RefreshToken=user.Refresh_token };
        }
        public async Task<UserDataDTO> GetUserByIdAsync(int id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id== id);
            if (user == null)
            {
                return null;
            }
            return new UserDataDTO { Email = user.Email, Id = user.Id, Role = user.Role, RefreshToken = user.Refresh_token };
        }
        public async Task<UserDataDTO> GetUserByLoginDataAsync(LoginDataDTO data)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email==data.Email && u.Password==data.Password);
            if (user == null)
            {
                return null;
            }
            return new UserDataDTO { Email = user.Email, Id = user.Id, Role = user.Role, RefreshToken = user.Refresh_token };
        }
        public async Task CreateUserAsync(RegisterDataDTO data)
        {
            await dbContext.Users.AddAsync(new DB.Models.User { Email=data.Email,Password=data.Password,Role=data.Role});
            await dbContext.SaveChangesAsync();
        }

        public async Task ChangeUserRefreshTokenAsync(int id,string refresh)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            user.Refresh_token= refresh;
            await dbContext.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
