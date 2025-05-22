using JobSeekerService.DB.DBContext;
using JobSeekerService.Domain.StatusService;
using JobSeekerService.Infrastructure.Features.Logger;
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
        private readonly IStatusService statusService;
        private readonly ILoggerService logger;

        public StatusController(IStatusService statusService, ILoggerService logger)
        {
            this.statusService=statusService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            logger.LogDebug("GetAllStatuses method called.");

            try
            {
                var resp = statusService.GetAllStatuses();
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
