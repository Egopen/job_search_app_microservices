using AuthService.DB.DBContext;
using AuthService.DB.Models;
using AuthService.Features;
using AuthService.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace JobSeekerService.Controllers
{
    [Route("Auth/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Context DB;
        private readonly LoggerService logger;

        public AuthController(Context context, LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterJobSeeker(RegisterRequest data)
        {
            logger.LogDebug("Register method called.");

            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            bool isValidEmail = Regex.IsMatch(data.Email, pattern);

            if (!isValidEmail)
            {
                logger.LogWarning("Invalid email format.");
                return BadRequest(new { error = "wrong email format" });
            }

            if (data.Password.Length <= 7)
            {
                logger.LogWarning("Password length is less than 8 characters.");
                return BadRequest(new { error = "password less than 8 symbols" });
            }

            var user = await DB.Users.FirstOrDefaultAsync(s => s.Email == data.Email);
            if (user != null)
            {
                logger.LogWarning("Attempt to register with an already registered email.");
                return BadRequest(new { error = "This email is already registered" });
            }

            try
            {
                var hashPasw = HashManager.CreateHash(data.Password);
                var us = new User { Password = hashPasw, Email = data.Email,Role="User" };
                await DB.Users.AddAsync(us);
                await DB.SaveChangesAsync();
                logger.LogInformation("New user registered successfully.");
                return Ok();
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

            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            bool isValidEmail = Regex.IsMatch(data.Email, pattern);

            if (!isValidEmail)
            {
                logger.LogWarning("Invalid email format.");
                return BadRequest(new { error = "wrong email format" });
            }

            if (data.Password.Length <= 7)
            {
                logger.LogWarning("Password length is less than 8 characters.");
                return BadRequest(new { error = "password less than 8 symbols" });
            }

            var user = await DB.Users.FirstOrDefaultAsync(s => s.Email == data.Email);
            if (user != null)
            {
                logger.LogWarning("Attempt to register with an already registered email.");
                return BadRequest(new { error = "This email is already registered" });
            }

            try
            {
                var hashPasw = HashManager.CreateHash(data.Password);
                var us = new User { Password = hashPasw, Email = data.Email, Role = "Employer" };
                await DB.Users.AddAsync(us);
                await DB.SaveChangesAsync();
                logger.LogInformation("New user registered successfully.");
                return Ok();
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

            var hashpas = HashManager.CreateHash(data.Password);
            var user = await DB.Users.FirstOrDefaultAsync(s => s.Email == data.Email && s.Password == hashpas);

            if (user == null)
            {
                logger.LogWarning("Failed login attempt with incorrect email or password.");
                return BadRequest(new { error = "Wrong password or email" });
            }

            try
            {
                var access = TokenManager.CreateAccessToken(user.Id.ToString(),user.Role);
                var refresh = TokenManager.CreateRefreshToken();
                user.Refresh_token = refresh;
                await DB.SaveChangesAsync();

                HttpContext.Response.Cookies.Append("Refresh", refresh, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(96),
                    HttpOnly = true,
                    Secure = true
                });

                logger.LogInformation("User logged in successfully.");
                return Ok(new { Access = access });
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
            logger.LogDebug("RefreshToken method called.");

            if (!HttpContext.Request.Cookies.ContainsKey("Refresh"))
            {
                logger.LogWarning("Refresh token is missing in the request.");
                return Forbid("Access denied.");
            }

            try
            {
                HttpContext.Request.Cookies.TryGetValue("Refresh", out var refreshToken);
                var userId = HttpContext.User.FindFirst("UserId")?.Value;

                if (userId == null)
                {
                    logger.LogWarning("User ID not found in the token.");
                    return Unauthorized("Wrong token.");
                }

                var user = await DB.Users.FirstOrDefaultAsync(s => s.Id == int.Parse(userId) && s.Refresh_token == refreshToken);
                if (user == null)
                {
                    logger.LogWarning("User not found or refresh token mismatch.");
                    return BadRequest(new { error = "something went wrong" });
                }

                var newRefresh = TokenManager.CreateRefreshToken();
                var newAccess = TokenManager.CreateAccessToken(userId,user.Role);
                user.Refresh_token = newRefresh;
                await DB.SaveChangesAsync();

                HttpContext.Response.Cookies.Append("Refresh", newRefresh, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(96)
                });

                logger.LogInformation("Refresh token updated successfully.");
                return Ok(new { AccessToken = newAccess });
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
            var userId = HttpContext.User.FindFirstValue("UserId");
            var role= HttpContext.User.Claims.FirstOrDefault(c => c.Type==ClaimTypes.Role)?.Value;
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId && u.Role == role);
            if (user == null) {

                return BadRequest(new { error = "Something went wrong" });
            }
            return Ok(new { email = user.Email, id=user.Id, role=user.Role });

        }
    }
}

