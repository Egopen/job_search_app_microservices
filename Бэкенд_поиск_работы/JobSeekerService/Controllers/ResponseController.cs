using JobSeekerService.DB.DBContext;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQInitializer;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text.Json;

namespace JobSeekerService.Controllers
{
    [Route("JobSeeker/[controller]/[action]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private Context DB;
        private LoggerService logger;
        private RabbitMQService rabbitMQService;
        public ResponseController(Context context, LoggerService logger,RabbitMQService rabbitMQService )
        {
            DB = context;
            this.logger = logger;
            this.rabbitMQService = rabbitMQService;
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddResponse(AddResponseToVacancyRequest addResponseJSON)
        {
            try
            {
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null || await DB.Resumes.FirstOrDefaultAsync((r)=>r.Id==addResponseJSON.Resume_id && r.Job_seekerId==int.Parse(userId))==null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await DB.Responses.AddAsync(new DB.Models.Response { ResumeId = addResponseJSON.Resume_id, Vacancy_id = addResponseJSON.Vacancy_id });
                await DB.SaveChangesAsync();
                var mes = JsonSerializer.Serialize(addResponseJSON);
                var body = System.Text.Encoding.UTF8.GetBytes(mes);
                await rabbitMQService.SendMessageAsync("seeker_response_queue", body);
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
                var res = await DB.Responses.FirstOrDefaultAsync((r) => r.Id==id);
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null || res == null || await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == res.ResumeId && r.Job_seekerId == int.Parse(userId)) == null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                DB.Responses.Remove(res);
                await DB.SaveChangesAsync();
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
                if (userId == null || await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == id && r.Job_seekerId == int.Parse(userId)) == null)
                {
                    logger.LogWarning("Something went wrong");
                    return BadRequest(new { error = "Something went wrong" });
                }
                var res = await (from r in DB.Responses
                          where r.ResumeId == id
                          select new ResponseResumeDataResponse
                          {
                              Id = r.Id,
                              Resume_id=r.ResumeId,
                              Vacancy_id=r.Vacancy_id
                          }).ToListAsync();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to get responses" });
            }
        }

    }
}
