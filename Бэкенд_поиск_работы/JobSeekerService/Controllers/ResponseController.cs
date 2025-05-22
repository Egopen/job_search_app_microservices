using JobSeekerService.DB.DBContext;
using JobSeekerService.Domain.RabbitMQ;
using JobSeekerService.Domain.ResponseService;
using JobSeekerService.Infrastructure.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text.Json;

namespace JobSeekerService.Controllers
{
    [Route("JobSeeker/[controller]/[action]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private ILoggerService logger;
        private IResponseService responseService;

        public ResponseController(IResponseService responseService, ILoggerService logger)
        {
            this.responseService = responseService;
            this.logger = logger;
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddResponse(AddResponseToVacancyRequest addResponseJSON)
        {
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await responseService.AddResponse(addResponseJSON, int.Parse(userId));
                return Ok();
            }
            catch (Exception ex) { 
                    return BadRequest(new {error="Failed to make response"});
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteResponseById(int id)
        {
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await responseService.DeleteResponseById(id,int.Parse(userId));
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to delete response" });
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> GetAllResponsesByResumeId(int id)
        {
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                var res = await responseService.GetAllResponsesByResumeId(id,int.Parse(userId));
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to get responses" });
            }
        }

    }
}
