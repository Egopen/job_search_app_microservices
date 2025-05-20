using EmployerService.DB.DBContext;
using EmployerService.Domain.Services.StatusService;
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
        private readonly ILoggerService logger;
        private readonly IStatusService statusService;

        public SatusController( ILoggerService logger,IStatusService statusService)
        {
            this.logger = logger;
            this.statusService = statusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            logger.LogInformation("Request received to fetch all statuses.");
            try
            {
                var statuses = await statusService.GetAllStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Something went wrong" });
            }
        }
    }
}
