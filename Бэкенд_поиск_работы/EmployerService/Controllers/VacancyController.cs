using EmployerService.DB.DBContext;
using EmployerService.JSON.RequestJSON;
using EmployerService.JSON.ResponseJSON;
using EmployerService.DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EmployerService.Domain.Services.VacancyService;
using EmployerService.Infrastructure.Features.Logger;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class VacancyController : ControllerBase
    {
        private readonly ILoggerService logger;
        private readonly IVacancyService vacancyService;

        public VacancyController( ILoggerService logger,IVacancyService vacancyService)
        {
            this.logger = logger;
            this.vacancyService = vacancyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBriefVacancy()
        {
            logger.LogInformation("Fetching all brief vacancies.");
            try
            {
                var vac = await vacancyService.GetAllBriefVacancy();
                logger.LogInformation($"Fetched {vac.Count()} brief vacancies successfully.");
                return Ok(vac);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching brief vacancies: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVacancyById(int id)
        {
            logger.LogInformation($"Fetching vacancy details for ID: {id}.");
            try
            {
                var vacancy = await vacancyService.GetVacancyById(id);

                logger.LogInformation($"Fetched vacancy details for ID {id} successfully.");
                return Ok(vacancy);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching vacancy by ID {id}: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployerBriefVacancy(int employer_id)
        {
            logger.LogInformation($"Fetching all brief vacancies for employer ID: {employer_id}.");
            try
            {
                var vac = await vacancyService.GetAllEmployerBriefVacancy(employer_id);
                return Ok(vac);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching vacancies for employer ID {employer_id}: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> GetAllOwnBriefVacancy()
        {
            logger.LogInformation("Fetching all brief vacancies for the authenticated employer.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                var vac = await vacancyService.GetAllOwnBriefVacancy(int.Parse(userId));
                return Ok(vac);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching employer's own vacancies: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpPost]
        public async Task<IActionResult> AddVacancy(AddVacancyRequest request)
        {
            logger.LogInformation("Adding a new vacancy.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await vacancyService.AddVacancy(request,int.Parse(userId));
                logger.LogInformation($"New vacancy added successfully by employer ID {userId}.");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while adding new vacancy: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpDelete]
        public async Task<IActionResult> DeleteVacancy(int vacancy_id)
        {
            logger.LogInformation($"Deleting vacancy with ID: {vacancy_id}.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await vacancyService.DeleteVacancy(vacancy_id,int.Parse(userId));
                logger.LogInformation($"Vacancy with ID {vacancy_id} deleted successfully.");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while deleting vacancy ID {vacancy_id}: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpPatch]
        public async Task<IActionResult> UpdateVacancy(UpdateVacancyRequest request)
        {
            logger.LogInformation($"Updating vacancy with ID: {request.Id}.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await vacancyService.UpdateVacancy(request,int.Parse(userId));
                logger.LogInformation($"Vacancy with ID {request.Id} updated successfully.");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while updating vacancy ID {request.Id}: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }
    }
}
