using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.RequestJSON;
using EmployerService.JSON.ResponseJSON;
using EmployerService.DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class VacancyController : ControllerBase
    {
        private readonly Context DB;
        private readonly LoggerService logger;

        public VacancyController(Context context, LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBriefVacancy()
        {
            logger.LogInformation("Fetching all brief vacancies.");
            try
            {
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies successfully.");
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
                var vacancy = await (from vc in DB.Vacancies
                                     join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                     join status in DB.Statuses on vc.StatusId equals status.Id
                                     where status.IsActive == true && vc.Id == id
                                     select new VacancyResponse
                                     {
                                         Id = vc.Id,
                                         Job_name = vc.Job_name,
                                         Experience_desc = exp.Desc,
                                         Description = vc.Description,
                                         Status_description = status.Desc,
                                         Experience_id = exp.Id,
                                         Status_id = vc.StatusId,
                                         Employer_id = vc.EmployerId,
                                         City = vc.City
                                     }).FirstOrDefaultAsync();

                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {id} not found.");
                    return NotFound(new { error = "Vacancy not found" });
                }

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
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true && vc.EmployerId == employer_id
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies for employer ID {employer_id} successfully.");
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
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true && vc.EmployerId == int.Parse(userId)
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies for authenticated employer successfully.");
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
                var vacancy = new Vacancy
                {
                    City = request.City,
                    Description = request.Description,
                    EmployerId = int.Parse(userId),
                    ExperienceId = request.Experience_id,
                    Job_name = request.Job_name,
                    StatusId = request.Status_id
                };
                await DB.Vacancies.AddAsync(vacancy);
                await DB.SaveChangesAsync();
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
                var vacancy = await DB.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancy_id && v.EmployerId == int.Parse(userId));
                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {vacancy_id} not found for deletion.");
                    return NotFound(new { error = "Vacancy not found" });
                }
                DB.Vacancies.Remove(vacancy);
                await DB.SaveChangesAsync();
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
                var vacancy = await DB.Vacancies.FirstOrDefaultAsync(v => v.Id == request.Id && v.EmployerId == int.Parse(userId));
                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {request.Id} not found for update.");
                    return NotFound(new { error = "Vacancy not found" });
                }

                if (!string.IsNullOrEmpty(request.Job_name)) vacancy.Job_name = request.Job_name;
                if (!string.IsNullOrEmpty(request.Description)) vacancy.Description = request.Description;
                if (!string.IsNullOrEmpty(request.City)) vacancy.City = request.City;
                if (request.Status_id > 0) vacancy.StatusId = request.Status_id;
                if (request.Experience_id > 0) vacancy.ExperienceId = request.Experience_id;

                await DB.SaveChangesAsync();
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
