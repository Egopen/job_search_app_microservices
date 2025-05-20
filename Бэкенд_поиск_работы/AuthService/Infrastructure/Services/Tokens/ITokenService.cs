using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Infrastructure.Services.Tokens
{
    public interface ITokenService
    {
        public string CreateAccessToken(string userId, string role);
        public string CreateRefreshToken();
    }
}
