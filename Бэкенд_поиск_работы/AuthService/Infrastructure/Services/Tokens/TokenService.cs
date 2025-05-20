using AuthService.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Infrastructure.Services.Tokens
{
    public class TokenService:ITokenService
    {
        public string CreateAccessToken(string userId, string role)
        {
            var claims = new List<Claim> { new("UserId", userId) };
            claims.Add(new Claim(ClaimTypes.Role, role));
            var jwt = new JwtSecurityToken(issuer: AuthOptions.ISSUER,// создание токенов для возвращения метода
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(2)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256));
            claims.Add(new Claim(ClaimTypes.Authentication, new JwtSecurityTokenHandler().WriteToken(jwt)));
            var AccesToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            return AccesToken;
        }
        public string CreateRefreshToken()
        {
            var jwt = new JwtSecurityToken(issuer: AuthOptions.ISSUER,// создание токенов для возвращения метода
            audience: AuthOptions.AUDIENCE,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(96)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256));
            var RefresToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            return RefresToken;
        }

    }
}
