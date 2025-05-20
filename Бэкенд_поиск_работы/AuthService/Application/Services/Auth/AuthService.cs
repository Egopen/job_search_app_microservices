using System.Security.Claims;
using System.Text.RegularExpressions;
using AuthService.Application.DTO;
using AuthService.Application.Exceptions;
using AuthService.Infrastructure.Services.DBProxy;
using AuthService.Infrastructure.Services.Hasher;
using AuthService.Infrastructure.Services.Logger;
using AuthService.Infrastructure.Services.Tokens;

namespace AuthService.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRespository userRespository;
        private readonly ILoggerService logger;
        private readonly IHashService hasher;
        private readonly ITokenService tokenManager;
        public AuthService(IUserRespository respository, ILoggerService logger, IHashService hasher, ITokenService tokenManager)
        {
            userRespository = respository;
            this.logger = logger;
            this.hasher = hasher;
            this.tokenManager = tokenManager;
        }
        public async Task RegisterUser(RegisterDataDTO data)
        {
            try
            {
                string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                bool isValidEmail = Regex.IsMatch(data.Email, pattern);

                if (!isValidEmail)
                {
                    logger.LogWarning("Invalid email format.");
                    throw new WrongDataException("Invalid email format.");
                }

                if (data.Password.Length <= 7)
                {
                    logger.LogWarning("Password length is less than 8 characters.");
                    throw new WrongDataException("Password length is less than 8 characters.");
                }

                var user = await userRespository.GetUserByEmailAsync(data.Email);
                if (user != null)
                {
                    logger.LogWarning("Attempt to register with an already registered email.");
                    throw new UserExistsExcpetion("Attempt to register with an already registered email.");
                }

                var refreshToken = tokenManager.CreateRefreshToken();
                var hashPasw = hasher.CreateHash(data.Password);
                data.Password = hashPasw;
                await userRespository.CreateUserAsync(data);
                logger.LogInformation("New user registered successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError("Error during registration", ex);
                throw;
            }
        }
        public async Task<TokenPairDTO> LoginUser(LoginDataDTO data)
        {
            try
            {
                var hashPas = hasher.CreateHash(data.Password);
                var user = await userRespository.GetUserByLoginDataAsync(data);
                if (user == null)
                {
                    logger.LogWarning("Failed login attempt with incorrect email or password.");
                    throw new UserNotExistsExcpetion("Failed login attempt with incorrect email or password.");
                }
                var access = tokenManager.CreateAccessToken(user.Id.ToString(), user.Role);
                var refresh = tokenManager.CreateRefreshToken();
                await userRespository.ChangeUserRefreshTokenAsync(user.Id, refresh);
                if (access != null && refresh != null)
                {
                    logger.LogInformation("User logged in successfully.");
                    return new TokenPairDTO { AccessToken = access, RefreshToken = refresh };
                }
                else
                {
                    throw new Exception("Tokens invalid");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error during login", ex);
                throw;
            }
        }
        public async Task<TokenPairDTO> RefreshToken(string refreshToken, int userId)
        {
            try
            {
                logger.LogDebug("RefreshToken method called.");
                var user = await userRespository.GetUserByIdAsync(userId);
                if (user == null || user.RefreshToken != refreshToken)
                {
                    logger.LogWarning("User not found or refresh token mismatch.");
                    throw new WrongDataException("Token is invalid");
                }

                var newRefresh = tokenManager.CreateRefreshToken();
                var newAccess = tokenManager.CreateAccessToken(userId.ToString(), user.Role);
                await userRespository.ChangeUserRefreshTokenAsync(userId, newRefresh);
                return new TokenPairDTO { AccessToken = newAccess, RefreshToken = newRefresh };
            }
            catch (Exception ex)
            {
                logger.LogError("Error during refresh token", ex);
                throw;
            }
        }
        public async Task<UserDataDTO> GetUserInfoFromToken(int id, string role)
        {
            try
            {
                var user = await userRespository.GetUserByIdAsync(id);
                if (user == null || user.Role != role)
                {
                    logger.LogWarning("User not found by data from access.");
                    throw new UserNotExistsExcpetion("User not found");
                }
                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
