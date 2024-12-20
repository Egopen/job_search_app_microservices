using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Controllers
{
    [Route("Employer/[controller]/[action]")]
    [ApiController]
    public class SatusController : ControllerBase
    {
        private readonly Context DB;
        private readonly LoggerService logger;

        public SatusController(Context context, LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            logger.LogInformation("Request received to fetch all statuses.");
            try
            {
                var statuses = new List<StatusResponse>();
                foreach (var stat in DB.Statuses)
                {
                    statuses.Add(new StatusResponse { Id = stat.Id, Desc = stat.Desc, Is_active = stat.IsActive });
                }

                logger.LogInformation($"Successfully fetched {statuses.Count} statuses from the database.");
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching statuses: {ex.Message}");
                return BadRequest(new { error = "Something went wrong" });
            }
        }
    }
}
