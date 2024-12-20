using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class ExperienceController : ControllerBase
    {
        private readonly Context DB;
        private readonly LoggerService logger;

        public ExperienceController(Context context, LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExperience()
        {
            logger.LogInformation("Received request to fetch all experiences.");

            try
            {
                var experiences = new List<ExperienceResponse>();

                foreach (var exp in DB.Experiences)
                {
                    experiences.Add(new ExperienceResponse { Id = exp.Id, Desc = exp.Desc });
                }

                logger.LogInformation($"Successfully fetched {experiences.Count} experiences.");
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
