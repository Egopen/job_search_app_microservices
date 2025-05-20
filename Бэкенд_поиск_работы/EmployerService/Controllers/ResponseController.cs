using EmployerService.DB.DBContext;
using EmployerService.Domain.Services.RabbitMQ;
using EmployerService.Domain.Services.ResponseService;
using EmployerService.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Text.Json;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private readonly IResponseService responseService;
        private readonly ILoggerService logger;

        public ResponseController(IResponseService responseService, ILoggerService logger)
        {
            this.responseService = responseService;
            this.logger = logger;
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "Employer")]
        [HttpDelete]
        public async Task<IActionResult> DeleteResponseById(int id)
        {
            logger.LogInformation($"Request received to delete response with ID: {id}.");
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning($"Delete operation failed. Either User ID is null, response not found, or access denied for User ID: {userId}.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await responseService.DeleteResponseById(id,int.Parse(userId));
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
                if (userId == null)
                {
                    logger.LogWarning($"Fetch operation failed. Either User ID is null or access denied for Vacancy ID: {id} and User ID: {userId}.");
                    return BadRequest(new { error = "Something went wrong" });
                }

                var res = await responseService.GetAllResponsesByVacancyId(id,int.Parse(userId));
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
