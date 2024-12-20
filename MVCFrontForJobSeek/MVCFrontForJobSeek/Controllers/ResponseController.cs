using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using static MVCFrontForJobSeek.Controllers.VacancyController;

namespace MVCFrontForJobSeek.Controllers
{
    public class ResponseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResponseController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Resume(int id)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("Access token is missing.");
            }
            ViewData["AccessToken"] = accessToken;
            // Создаем HTTP клиент
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Получаем отклики по ID резюме
            var responsesUrl = $"http://localhost:8080/gateway/JobSeeker/Response/GetAllResponsesByResumeId?id={id}";
            var responsesResponse = await client.GetAsync(responsesUrl);
            if (!responsesResponse.IsSuccessStatusCode)
            {
                return BadRequest("Failed to fetch responses.");
            }
            var responses = await responsesResponse.Content.ReadFromJsonAsync<List<ResponseViewModel>>();

            // Получаем данные о резюме и вакансиях
            var resumes = new List<ResumeViewModel>();
            var vacancies = new List<VacancyViewModel>();

            foreach (var response in responses)
            {
                // Получение резюме
                var resumeResponse = await client.GetAsync($"http://localhost:8080/gateway/JobSeeker/Resume/GetResumeById?id={response.Resume_id}");
                if (resumeResponse.IsSuccessStatusCode)
                {
                    var resume = await resumeResponse.Content.ReadFromJsonAsync<ResumeViewModel>();
                    resumes.Add(resume);
                }

                // Получение вакансии
                var vacancyResponse = await client.GetAsync($"http://localhost:8080/gateway/Employer/Vacancy/GetVacancyById?id={response.Vacancy_id}");
                if (vacancyResponse.IsSuccessStatusCode)
                {
                    var vacancy = await vacancyResponse.Content.ReadFromJsonAsync<VacancyViewModel>();
                    vacancies.Add(vacancy);
                }
            }

            // Передаем данные в представление
            var responseData = new CombinedResponseViewModel
            {
                Responses = responses,
                Resumes = resumes,
                Vacancies = vacancies
            };

            return View(responseData);
        }
        public async Task<IActionResult> Vacancy(int id)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("Access token is missing.");
            }
            ViewData["AccessToken"] = accessToken;
            // Создаем HTTP клиент
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Получаем отклики по ID резюме
            var responsesUrl = $"http://localhost:8080/gateway/Employer/Response/GetAllResponsesByVacancyId?id={id}";
            var responsesResponse = await client.GetAsync(responsesUrl);
            if (!responsesResponse.IsSuccessStatusCode)
            {
                return BadRequest("Failed to fetch responses.");
            }
            var responses = await responsesResponse.Content.ReadFromJsonAsync<List<ResponseViewModel>>();

            // Получаем данные о резюме и вакансиях
            var resumes = new List<ResumeViewModel>();
            var vacancies = new List<VacancyViewModel>();

            foreach (var response in responses)
            {
                // Получение резюме
                var resumeResponse = await client.GetAsync($"http://localhost:8080/gateway/JobSeeker/Resume/GetResumeById?id={response.Resume_id}");
                if (resumeResponse.IsSuccessStatusCode)
                {
                    var resume = await resumeResponse.Content.ReadFromJsonAsync<ResumeViewModel>();
                    resumes.Add(resume);
                }

                // Получение вакансии
                var vacancyResponse = await client.GetAsync($"http://localhost:8080/gateway/Employer/Vacancy/GetVacancyById?id={response.Vacancy_id}");
                if (vacancyResponse.IsSuccessStatusCode)
                {
                    var vacancy = await vacancyResponse.Content.ReadFromJsonAsync<VacancyViewModel>();
                    vacancies.Add(vacancy);
                }
            }

            // Передаем данные в представление
            var responseData = new CombinedResponseViewModel
            {
                Responses = responses,
                Resumes = resumes,
                Vacancies = vacancies
            };

            return View(responseData);
        }
    }

    // Модели для работы с API
    public class ResponseViewModel
    {
        public int Id { get; set; }
        public int Resume_id { get; set; }
        public int Vacancy_id { get; set; }
    }

    public class ResumeViewModel
    {
        public int Id { get; set; }
        public string? Desc { get; set; }
        public string? Mobile_phone { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Job_name { get; set; }
        public int Salary { get; set; }
    }

    public class VacancyViewModel
    {
        public int Id { get; set; }
        public string Job_name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Experience_desc { get; set; }
    }

    public class CombinedResponseViewModel
    {
        public List<ResumeViewModel> Resumes { get; set; }
        public List<VacancyViewModel> Vacancies { get; set; }
        public List<ResponseViewModel> Responses { get; set; }
    }
}
