using AuthService.Application.DTO;
using AuthService.Application.Services.Auth;
using AuthService.Infrastructure.DB.DBContext;
using AuthService.Infrastructure.DB.Models;
using AuthService.Infrastructure.Services.Hasher;
using AuthService.Infrastructure.Services.Logger;
using AuthService.Infrastructure.Services.Tokens;
using AuthService.UI.DTO.RequestJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AuthService.UI.Controllers
{
    [Route("Auth/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Context DB;
        private readonly ILoggerService logger;
        private readonly IHashService hasher;
        private readonly ITokenService tokenManager;
        private readonly IAuthService authService;

        public AuthController(Context context, ILoggerService logger, IHashService hasher, ITokenService tokenManager,IAuthService authService)
        {
            DB = context;
            this.logger = logger;
            this.hasher = hasher;
            this.tokenManager = tokenManager;
            this.authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterJobSeeker(RegisterRequest data)
        {
            logger.LogDebug("Register method called.");
            try
            {
                await authService.RegisterUser(new RegisterDataDTO { Email=data.Email,Password=data.Password,Role="User"});
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during registration", ex);
                return BadRequest(new { error = "something went wrong" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> RegisterEmployer(RegisterRequest data)
        {
            logger.LogDebug("Register method called.");
            try
            {
                await authService.RegisterUser(new RegisterDataDTO { Email = data.Email, Password = data.Password, Role = "Employer" });
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during registration", ex);
                return BadRequest(new { error = "something went wrong" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(RegisterRequest data)
        {
            logger.LogDebug("Login method called.");


            try
            {
                var tokenPair = await authService.LoginUser(new LoginDataDTO { Email=data.Email,Password=data.Password}); 
                HttpContext.Response.Cookies.Append("Refresh", tokenPair.RefreshToken, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(96),
                    HttpOnly = true,
                    Secure = true
                });

                logger.LogInformation("User logged in successfully.");
                return Ok(new { Access = tokenPair.AccessToken });
            }
            catch (Exception ex)
            {
                logger.LogError("Error during login", ex);
                return BadRequest(new { error = "something went wrong" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                logger.LogDebug("RefreshToken method called.");
                if (!HttpContext.Request.Cookies.ContainsKey("Refresh"))
                {
                    logger.LogWarning("Refresh token is missing in the request.");
                    return Forbid("Access denied.");
                }
                HttpContext.Request.Cookies.TryGetValue("Refresh", out var refreshToken);
                var userId = HttpContext.User.FindFirst("UserId")?.Value;
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in the token.");
                    return Unauthorized("Wrong token.");
                }
                var tokenPair = await authService.RefreshToken(refreshToken,int.Parse(userId));
                HttpContext.Response.Cookies.Append("Refresh", tokenPair.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(96)
                });

                logger.LogInformation("Refresh token updated successfully.");
                return Ok(new { AccessToken = tokenPair.AccessToken });
            }
            catch (Exception ex)
            {
                logger.LogError("Error during refresh token", ex);
                return BadRequest(new { error = ex.Message });
            }
        }
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpGet]
        public async Task<IActionResult> GetUserInfoFromToken()
        {
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var user = await authService.GetUserInfoFromToken(int.Parse(userId), role);
                if (user == null)
                {

                    return BadRequest(new { error = "Something went wrong" });
                }
                return Ok(new { email = user.Email, id = user.Id, role = user.Role });
            }
            catch (Exception ex)
            {
                logger.LogError("Error during refresh token", ex);
                return BadRequest(new { error = ex.Message });
            }

        }
    }
}

