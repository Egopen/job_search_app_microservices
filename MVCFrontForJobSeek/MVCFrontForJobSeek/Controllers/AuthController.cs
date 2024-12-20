using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace MVCFrontForJobSeek.Controllers
{
    public class AuthController : Controller
    {

        public AuthController()
        {
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterJobSeeker([FromBody] RegisterRequest data)
        {
            // Обращение к backend API для регистрации соискателя
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:8080/");
            var response = await client.PostAsJsonAsync("gateway/Auth/Auth/RegisterJobSeeker", data);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<object>();
                return BadRequest(error);
            }
            return Ok("User registered");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterEmployer([FromBody] RegisterRequest data)
        {
            // Обращение к backend API для регистрации работодателя
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5003/"); // URL вашего backend API
            var response = await client.PostAsJsonAsync("gateway/Auth/Auth/RegisterEmployer", data);
            var error = await response.Content.ReadFromJsonAsync<object>();
            return BadRequest(error);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest data)
        {
            // Обращение к backend API для входа
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:8080/"); // URL вашего backend API
            var response = await client.PostAsJsonAsync("gateway/Auth/Auth/Login", data);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                // Сохраняем токен в куки
                Response.Cookies.Append("AccessToken", result.Access, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(2)
                });

                return RedirectToAction("Index", "Resume");
            }

            var error = await response.Content.ReadFromJsonAsync<object>();
            return BadRequest(error);
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            // Удаляем токен из cookies
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login");
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Access { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
