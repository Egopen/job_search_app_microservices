using Microsoft.AspNetCore.Mvc;
using MVCFrontForJobSeek.Models;
using System.Text;
using System.Text.Json;

namespace MVCFrontForJobSeek.Controllers
{
    [Route("[controller]")]
    public class VacancyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VacancyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        // Страница для отображения списка вакансий
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();  // Возвращает представление Vacancy/Index.cshtml
        }

        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var token = Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                // Получаем информацию о пользователе
                var userInfo = await GetUserInfoFromToken(token);
                if (userInfo != null && userInfo.Role == "User")
                {
                    var resumes = await GetUserResumes(token);
                    ViewData["Resumes"] = resumes;
                    ViewData["UserInfo"] = userInfo;
                }
            }
            return View();
        }

        private async Task<UserInfo> GetUserInfoFromToken(string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.GetAsync("http://localhost:8080/gateway/Auth/Auth/GetUserInfoFromToken");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfo>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return userInfo;
            }

            return null;
        }

        private async Task<List<Resume>> GetUserResumes(string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.GetAsync("http://localhost:8080/gateway/JobSeeker/Resume/GetAllUserBriefResume");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var resumes = JsonSerializer.Deserialize<List<Resume>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return resumes;
            }

            return new List<Resume>(); // Возвращаем пустой список, если не удалось получить резюме
        }

        // Обработка отклика на вакансию
        [HttpPost]
        [Route("ApplyForVacancy")]
        public async Task<IActionResult> ApplyForVacancy([FromBody] ResponseModel responseModel)
        {
            var token = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var jsonResponse = JsonSerializer.Serialize(responseModel);
            var content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:8080/gateway/JobSeeker/Response/AddResponse", content);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Отклик успешно отправлен!" });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = errorMessage });
            }
        }

        // Модели данных
        public class UserInfo
        {
            public string Email { get; set; }
            public int Id { get; set; }
            public string Role { get; set; }
        }

        public class Resume
        {
            public int Id { get; set; }
            public int Salary { get; set; }
            public string JobName { get; set; }
            public string City { get; set; }
            public string Experience { get; set; }
        }

        public class ResponseModel
        {
            public int Resume_id { get; set; }
            public int Vacancy_id { get; set; }
        }
        [HttpGet]
        [Route("AddVacancy")]
        public IActionResult AddVacancy()
        {
            var accessToken = Request.Cookies["AccessToken"];  // Метод получения токена
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewData["AccessToken"] = accessToken;  // Сохраняем токен в ViewData
            return View();
        }

        // Обработка POST-запроса для добавления вакансии
        [HttpPost]
        [Route("AddVacancy")]
        public async Task<IActionResult> AddVacancy([FromBody] VacancyModel model)
        {
            if (ModelState.IsValid)
            {
                var client = new HttpClient();
                var requestUrl = "http://localhost:8080/gateway/Employer/Vacancy/AddVacancy";
                var accessToken = Request.Headers["Authorization"];  // Получаем токен из заголовков

                var json = System.Text.Json.JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                content.Headers.TryAddWithoutValidation("Authorization", accessToken.ToString()); ;  // Добавляем токен в заголовки

                var response = await client.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Вакансия успешно добавлена!" });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = errorMessage });
                }
            }

            return Json(new { success = false, message = "Ошибка валидации данных!" });
        }
        [Route("SelfVacancyDetails/{id}")]
        [HttpGet]
        public async Task<IActionResult> SelfVacancyDetails(int id)
        {
            var token = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Передаем токен в ViewData
            ViewData["AccessToken"] = token;

            return View(); ;

        }
    }
}
