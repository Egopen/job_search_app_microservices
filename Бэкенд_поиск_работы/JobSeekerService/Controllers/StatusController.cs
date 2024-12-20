using JobSeekerService.DB.DBContext;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeekerService.Controllers
{
    [Route("JobSeeker/[controller]/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly Context DB;
        private readonly LoggerService logger;

        public StatusController(Context context, LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            logger.LogDebug("GetAllStatuses method called.");

            try
            {
                var resp = new List<StatusResponse>();
                var statuses = await DB.Statuses.ToListAsync();  // Асинхронное получение данных из базы

                foreach (var status in statuses)
                {
                    resp.Add(new StatusResponse { Id = status.Id, Desc = status.Desc });
                }

                logger.LogInformation($"Successfully retrieved {resp.Count} statuses.");
                return Ok(resp);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving statuses.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving statuses." });
            }
        }
    }
}
