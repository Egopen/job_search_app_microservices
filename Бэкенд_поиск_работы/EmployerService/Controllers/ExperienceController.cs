using EmployerService.DB.DBContext;
using EmployerService.Domain.Services.ExperienceService;
using EmployerService.Infrastructure.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class ExperienceController : ControllerBase
    {
        private readonly ILoggerService logger;
        private readonly IExperienceService experienceService;

        public ExperienceController( ILoggerService logger,IExperienceService experienceService)
        {
            this.logger = logger;
            this.experienceService = experienceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExperience()
        {
            logger.LogInformation("Received request to fetch all experiences.");

            try
            {
                var experiences = experienceService.GetAllExperience();
                return Ok(experiences);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching experiences: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }
    }
}
