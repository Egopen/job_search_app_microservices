using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQInitializer;
using System.Security.Claims;
using System.Text.Json;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private Context DB;
        private LoggerService logger;
        private RabbitMQService rabbitMQService;

        public ResponseController(Context context, LoggerService logger, RabbitMQService rabbitMQService)
        {
            DB = context;
            this.logger = logger;
            this.rabbitMQService = rabbitMQService;
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpDelete]
        public async Task<IActionResult> DeleteResponseById(int id)
        {
            logger.LogInformation($"Request received to delete response with ID: {id}.");
            try
            {
                var res = await DB.Responses.FirstOrDefaultAsync((r) => r.Id == id);
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null || res == null ||
                    await DB.Vacancies.FirstOrDefaultAsync((v) => v.Id == res.VacancyId && v.EmployerId == int.Parse(userId)) == null)
                {
                    logger.LogWarning($"Delete operation failed. Either User ID is null, response not found, or access denied for User ID: {userId}.");
                    return BadRequest(new { error = "Something went wrong" });
                }

                DB.Responses.Remove(res);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Response with ID {id} successfully deleted by User ID: {userId}.");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while deleting response with ID {id}: {ex.Message}");
                return BadRequest(new { error = "Failed to delete response" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> GetAllResponsesByVacancyId(int id)
        {
            logger.LogInformation($"Request received to fetch all responses for vacancy with ID: {id}.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null ||
                    await DB.Vacancies.FirstOrDefaultAsync((v) => v.Id == id && v.EmployerId == int.Parse(userId)) == null)
                {
                    logger.LogWarning($"Fetch operation failed. Either User ID is null or access denied for Vacancy ID: {id} and User ID: {userId}.");
                    return BadRequest(new { error = "Something went wrong" });
                }

                var res = await (from r in DB.Responses
                                 where r.VacancyId == id
                                 select new ResponseVacancyDataResponse
                                 {
                                     Id = r.Id,
                                     Resume_id = r.Resume_id,
                                     Vacancy_id = r.VacancyId
                                 }).ToListAsync();

                logger.LogInformation($"Successfully fetched {res.Count} responses for Vacancy ID: {id} by User ID: {userId}.");
                return Ok(res);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching responses for Vacancy ID {id}: {ex.Message}");
                return BadRequest(new { error = "Failed to get responses" });
            }
        }
    }
}
