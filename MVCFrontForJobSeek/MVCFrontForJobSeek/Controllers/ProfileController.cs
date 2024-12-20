using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MVCFrontForJobSeek.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(HttpClient httpClient, ILogger<ProfileController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                // Получаем токен из cookies
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Извлекаем UserId и роль из токена JWT
                var userId = GetUserIdFromToken(token);
                var role = GetRoleFromToken(token);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                {
                    return View("Error"); // Если не удалось извлечь UserId или роль
                }

                // Настройка заголовка с токеном
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Запрос на получение данных о пользователе по UserId
                var response = await _httpClient.GetAsync($"http://localhost:8080/gateway/Auth/Auth/GetUserInfoFromToken");
                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadFromJsonAsync<UserData>();
                    userData.Role = role;  // Добавляем роль в данные пользователя

                    // Запросы в зависимости от роли
                    if (role == "User")
                    {
                        var resumesResponse = await _httpClient.GetAsync($"http://localhost:8080/gateway/JobSeeker/Resume/GetAllUserBriefResume");
                        if (resumesResponse.IsSuccessStatusCode)
                        {
                            var resumes = await resumesResponse.Content.ReadFromJsonAsync<List<Resume>>();
                            userData.Resumes = resumes;
                        }
                    }
                    else if (role == "Employer")
                    {
                        var vacanciesResponse = await _httpClient.GetAsync($"http://localhost:8080/gateway/Employer/Vacancy/GetAllOwnBriefVacancy");
                        if (vacanciesResponse.IsSuccessStatusCode)
                        {
                            var vacancies = await vacanciesResponse.Content.ReadFromJsonAsync<List<Vacancy>>();
                            userData.Vacancies = vacancies;
                        }
                    }

                    return View(userData); // Передаем данные на страницу профиля
                }

                // Логирование ошибки
                _logger.LogError("Error fetching user data from API: {StatusCode}", response.StatusCode);
                return View("Error"); // В случае ошибки
            }
            catch (Exception ex)
            {
                // Логируем ошибку с полными деталями
                _logger.LogError(ex, "An error occurred while processing the profile request.");
                return View("Error"); // Отображаем страницу с ошибкой
            }
        }

        // Метод для извлечения UserId из токена
        private string GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting UserId from token.");
                return null;
            }
        }

        // Метод для извлечения роли из токена
        private string GetRoleFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var role = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting role from token.");
                return null;
            }
        }
    }

    // Классы для десериализации данных
    public class UserData
    {
        public string Email { get; set; }
        public int Id { get; set; }
        public string Role { get; set; }
        public List<Resume> Resumes { get; set; }
        public List<Vacancy> Vacancies { get; set; }
    }

    public class Resume
    {
        public int Id { get; set; }
        public int Salary { get; set; }
        public string JobName { get; set; }
        public string City { get; set; }
        public string Experience { get; set; }
    }

    public class Vacancy
    {
        public int Id { get; set; }
        public string Job_name { get; set; }
        public string City { get; set; }
        public int Employer_id { get; set; }
        public int Status_id { get; set; }
        public int Experience_id { get; set; }
        public string Experience_desc { get; set; }
    }
}
