using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCFrontForJobSeek.JSON;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MVCFrontForJobSeek.Controllers
{
    public class ResumeController : Controller
    {
        private readonly HttpClient _httpClient;

        public ResumeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var apiUrl = "http://localhost:8080/gateway/JobSeeker/Resume/GetAllBriefResume";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Ошибка при получении данных.";
                    return View(new List<BriefResume>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var resumes = JsonSerializer.Deserialize<List<BriefResume>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(resumes);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(new List<BriefResume>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var apiUrl = $"http://localhost:8080/gateway/JobSeeker/Resume/GetResumeById?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Ошибка при получении данных о резюме.";
                    return View(new ResumeDetails());
                }

                var content = await response.Content.ReadAsStringAsync();
                var resumeDetails = JsonSerializer.Deserialize<ResumeDetails>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(resumeDetails);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(new ResumeDetails());
            }
        }
        public async Task<IActionResult> SelfDetailsResume(int id)
        {
            try
            {
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Передаем токен в ViewData
                ViewData["AccessToken"] = token;
                var apiUrl = $"http://localhost:8080/gateway/JobSeeker/Resume/GetResumeById?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Ошибка при получении данных о резюме.";
                    return View(new ResumeDetails());
                }

                var content = await response.Content.ReadAsStringAsync();
                var resumeDetails = JsonSerializer.Deserialize<ResumeDetails>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(resumeDetails);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(new ResumeDetails());
            }
        }
        public IActionResult Create()
        {
            var token = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Передаем токен в ViewData
            ViewData["AccessToken"] = token;

            return View(new ResumeRequest());
        }
        public async Task<IActionResult> EditResume(int id)
        {
            try
            {
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var apiUrl = $"http://localhost:8080/gateway/JobSeeker/Resume/GetResumeById?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Ошибка при получении данных о резюме.";
                    return View(new UpdateResumeRequest());
                }

                var content = await response.Content.ReadAsStringAsync();
                var resumeDetails = JsonSerializer.Deserialize<UpdateResumeRequest>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(resumeDetails);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(new UpdateResumeRequest());
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditResume(UpdateResumeRequest model)
        {
            try
            {
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var apiUrl = "http://localhost:8080/gateway/JobSeeker/Resume/UpdateResume";
                var requestContent = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PatchAsync(apiUrl, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Резюме успешно обновлено.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Ошибка при обновлении резюме: {errorContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(model);
            }
        }
        public IActionResult AddExperience(int resumeId)
        {
            // Проверяем, что resumeId передается корректно
            if (resumeId == 0)
            {
                ViewBag.Error = "Ошибка: не передан ID резюме.";
                return View();
            }

            // Создаем модель с переданным ResumeId
            var model = new ExperienceRequest
            {
                ResumeId = resumeId  // Передаем ID резюме в модель
            };

            return View(model);
        }

        // POST-метод для обработки добавления опыта
        [HttpPost]
        public async Task<IActionResult> AddExperience(ExperienceRequest model)
        {
            try
            {
                // Проверка на наличие токена
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Проверяем, что модель валидна
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Проверяем, что ResumeId передан
                if (model.ResumeId == 0)
                {
                    ViewBag.Error = "Ошибка: не указан ID резюме.";
                    return View(model);
                }

                // Формируем запрос на сервер для добавления опыта
                var apiUrl = "http://localhost:8080/gateway/JobSeeker/Resume/AddExperience";
                var requestContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync(apiUrl, requestContent);

                // Если запрос прошел успешно, перенаправляем на другую страницу
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Опыт успешно добавлен.";
                    return RedirectToAction("Index");  // Здесь можно перенаправить на другую страницу
                }

                // Если произошла ошибка, выводим сообщение об ошибке
                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Ошибка при добавлении опыта: {errorContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                // Обработка исключений
                ViewBag.Error = "Произошла ошибка: " + ex.Message;
                return View(model);
            }
        }

        public class ResumeRequest
        {
            public string Desc { get; set; } = "";
            [Required]
            public string Name { get; set; }
            [Required]
            public string MobilePhone { get; set; }
            [Required]
            public string City { get; set; }
            [Required]
            public int StatusId { get; set; }
            [Required]
            public string JobName { get; set; }
            [Required]
            public uint Salary { get; set; }
        }
        public class ExperienceRequest
        {
            [Required]
            public DateOnly Start_d { get; set; }

            [Required]
            public DateOnly Finish_d { get; set; }

            [Required]
            public string Company_name { get; set; }

            [Required]
            public string Job_name { get; set; }

            [Required]
            public string City { get; set; }

            public string? Desc { get; set; }

            [Required]
            public int ResumeId { get; set; }
        }
        public class UpdateResumeRequest
        {
            [Required]
            public int Id { get; set; } // Идентификатор резюме
            public string? Desc { get; set; } // Описание
            public string? Mobile_phone { get; set; } // Мобильный телефон
            public string? Name { get; set; } // Имя
            public string? City { get; set; } // Город
            public string? Job_name { get; set; } // Название должности
            public int Salary { get; set; } // Зарплата
            public int StatusId { get; set; } // ID статуса
        }
    }
}
