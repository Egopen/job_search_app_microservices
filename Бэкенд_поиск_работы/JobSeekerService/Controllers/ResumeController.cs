using JobSeekerService.DB.DBContext;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using JobSeekerService.DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace JobSeekerService.Controllers
{
    [Route("JobSeeker/[controller]/[action]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private Context DB;
        private LoggerService logger;
        public ResumeController(Context context,LoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateResume(ResumeRequest request)
        {
            logger.LogDebug("CreateResume method called.");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                logger.LogWarning("User ID not found in token.");
                return BadRequest(new { error = "Something went wrong" });
            }
            if (!Regex.IsMatch(request.MobilePhone, @"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)"))
            {
                logger.LogWarning("Invalid mobile phone format provided.");
                return BadRequest(new { error = "Wrong mobile phone format" });
            }
            if (!Regex.IsMatch(request.City, @"^[a-zA-Zа-яА-Я\s\-\+#]+$"))
            {
                logger.LogWarning("Invalid city name format provided.");
                return BadRequest(new { error = "Wrong city name" });
            }
            if (!Regex.IsMatch(request.JobName, @"^[a-zA-Zа-яА-Я\s-]+$"))
            {
                logger.LogWarning("Invalid job name format provided.");
                return BadRequest(new { error = "Wrong job name" });
            }
            var st = await DB.Statuses.FirstOrDefaultAsync((s) => s.Id == request.StatusId);
            if (st == null)
            {
                logger.LogWarning("Status ID does not exist.");
                return BadRequest(new { error = "No such status" });
            }
            try
            {
                await DB.Resumes.AddAsync(new Resume
                {
                    City = request.City,
                    Desc = request.Desc,
                    Mobile_phone = request.MobilePhone,
                    Job_name = request.JobName,
                    Job_seekerId = int.Parse(userId),
                    StatusId = request.StatusId,
                    Salary = ((int)(request.Salary)),
                    Name= request.Name
                });
                await DB.SaveChangesAsync();
                logger.LogInformation("Resume created successfully for user ID " + userId);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("Error while creating resume", ex);
                return BadRequest(new { error = "Something went wrong" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> GetAllUserBriefResume()
        {
            logger.LogDebug("GetAllUserBriefResume method called.");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                logger.LogWarning("User ID not found in token.");
                return BadRequest(new { error = "Something went wrong" });
            }
            try
            {
                var resumes = await (from resume in DB.Resumes
                              join experience in DB.Experience
                              on resume.Id equals experience.ResumeId into experienceGroup
                              where resume.Job_seekerId==int.Parse(userId)
                              select new ResponseBriefResume
                              {
                                  Id = resume.Id,
                                  Salary = resume.Salary,
                                  JobName = resume.Job_name,
                                  City = resume.City,
                                  Experience= (experienceGroup.Any() ? Math.Round(experienceGroup.Sum(e=>(e.Finish_d.ToDateTime(TimeOnly.MinValue) - e.Start_d.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.0) * 2,
                                  MidpointRounding.AwayFromZero) / 2 : 0).ToString()
                              }).ToListAsync();
                              
                logger.LogInformation($"Found {resumes.Count} resumes for user ID {userId}.");
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving user resumes.", ex);
                return BadRequest(new { error = "Something went wrong" });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetResumeById(int id)
        {
            logger.LogDebug($"GetResumeById method called with ID: {id}");
            var resumes = await (from resume in DB.Resumes
                                 join experience in DB.Experience
                                 on resume.Id equals experience.ResumeId into experienceGroup
                                 join status in DB.Statuses
                                 on resume.StatusId equals status.Id
                                 where resume.Id==id
                                 select new ResponseResume
                                 {
                                     Id = resume.Id,
                                     Salary = resume.Salary,
                                     Job_name = resume.Job_name,
                                     City = resume.City,
                                     Desc=resume.Desc,
                                     Job_seekerId = resume.Job_seekerId,
                                     Mobile_phone = resume.Mobile_phone,
                                     Status=status.Desc,
                                     StatusId=resume.StatusId,
                                     Experience = (experienceGroup.Any() ? Math.Round(experienceGroup.Sum(e => (e.Finish_d.ToDateTime(TimeOnly.MinValue) - e.Start_d.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.0) * 2,
                                     MidpointRounding.AwayFromZero) / 2 : 0).ToString(),
                                     Name= resume.Name
                                 }).ToListAsync();
            var result = resumes.FirstOrDefault();
            if (result == null)
            {
                logger.LogWarning($"No resume found for ID: {id}");
                return BadRequest(new { error = "No such reusme" });
            }
            logger.LogInformation($"Resume found for ID: {id}");
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBriefResume()
        {
            logger.LogDebug("GetAllBriefResume method called.");
            try
            {
                var resumes = await (from resume in DB.Resumes
                                     join experience in DB.Experience
                                     on resume.Id equals experience.ResumeId into experienceGroup
                                     select new ResponseBriefResume
                                     {
                                         Id = resume.Id,
                                         Salary = resume.Salary,
                                         JobName = resume.Job_name,
                                         City = resume.City,
                                         Experience = (experienceGroup.Any() ? Math.Round(experienceGroup.Sum(e => (e.Finish_d.ToDateTime(TimeOnly.MinValue) - e.Start_d.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.0) * 2,
                                         MidpointRounding.AwayFromZero) / 2 : 0).ToString()
                                     }).ToListAsync();
                logger.LogInformation("Successfully generated response for all brief resumes.");
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving all brief resumes.", ex);
                return BadRequest(new { error = "Something went wrong" });
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPatch]
        public async Task<IActionResult> UpdateResume(UpdateResumeRequest request)
        {
            logger.LogDebug($"UpdateResume method called for Resume ID: {request.Id}");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return BadRequest(new { error = "Something went wrong" });
            }
            var resume = await DB.Resumes.FirstOrDefaultAsync((r)=>r.Id==request.Id && r.Job_seekerId==int.Parse(userId));
            if (resume == null)
            {
                return NotFound(new { error = "Resume not found" });
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                resume.Name = request.Name;
            }
            if (!string.IsNullOrEmpty(request.Desc))
            {
                resume.Desc = request.Desc;
            }
            if (!string.IsNullOrEmpty(request.Mobile_phone))
            {
                resume.Mobile_phone = request.Mobile_phone;
            }
            if (!string.IsNullOrEmpty(request.City))
            {
                resume.City = request.City;
            }
            if (!string.IsNullOrEmpty(request.Job_name))
            {
                resume.Job_name = request.Job_name;
            }
            if (request.Salary != 0) // Если зарплата передана и не 0
            {
                resume.Salary = request.Salary;
            }
            if (request.StatusId != 0) // Если статус передан и не 0
            {
                resume.StatusId = request.StatusId;
            }
            try
            {
                await DB.SaveChangesAsync();
                logger.LogInformation($"Resume ID: {request.Id} successfully updated.");
                return Ok();
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while updating Resume ID: {request.Id}", ex);
                return BadRequest(new { error = "Something went wrong" });
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteResume(int resumeId)
        {
            logger.LogDebug($"DeleteResume method called for Resume ID: {resumeId}");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return BadRequest(new { error = "Something went wrong" });
            }
            var resume = await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == resumeId && r.Job_seekerId == int.Parse(userId));
            if (resume == null)
            {
                logger.LogWarning($"Resume with ID {resumeId} not found for user ID {userId}.");
                return NotFound(new { error="Resume not found"});
            }
            try
            {
                DB.Resumes.Remove(resume);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Resume ID: {resumeId} successfully deleted.");
                return Ok();
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while deleting Resume ID: {resumeId}", ex);
                return BadRequest(new { error = "Не удалось удалить резюме" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddExperience(ExperienceRequest request)
        {
            logger.LogDebug("AddExperience method called");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                logger.LogWarning("User ID not found in the token.");
                return BadRequest(new { error = "Something went wrong" });
            }
            var resume= await DB.Resumes.FirstOrDefaultAsync((r)=> r.Id==request.ResumeId && r.Job_seekerId==int.Parse(userId));
            if (resume == null)
            {
                logger.LogWarning($"Resume with ID {request.ResumeId} does not belong to user ID {userId}.");
                return BadRequest(new { error = "This resume doesnt belong to you" });
            }
            if (request.Start_d > request.Finish_d || request.Start_d.ToDateTime(TimeOnly.MinValue)>DateTime.UtcNow)
            {
                return BadRequest(new { error = "Wrong period of work" });
            }
            try
            {

                await DB.Experience.AddAsync(new Experience
                {
                    ResumeId = request.ResumeId,
                    Job_name = request.Job_name,
                    City = request.City,
                    Company_name = request.Company_name,
                    Desc = request.Desc,
                    Finish_d = request.Finish_d,
                    Start_d = request.Start_d
                });
                await DB.SaveChangesAsync();
                logger.LogInformation($"Experience for Resume ID {request.ResumeId} successfully added.");
                return Ok();
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while adding experience for Resume ID {request.ResumeId}: {ex.Message}", ex);
                return BadRequest(new { error = "Не удалось добавить опыт" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExperienceByResumeId(int resume_id)
        {
            logger.LogDebug($"GetAllExperienceByResumeId called with ResumeId: {resume_id}");
            try
            {
                var exps= await DB.Experience.Where((r)=>r.ResumeId==resume_id).ToListAsync();
                if (!exps.Any())
                {
                    logger.LogWarning($"No experience found for ResumeId: {resume_id}");
                    return BadRequest(new { error = "Не удалось получить связанные данные об опыте" });
                }
                var expresp=new List<ExperienceResponse>();
                foreach (var experience in exps) {
                    expresp.Add(new ExperienceResponse
                    {
                        Id = experience.Id,
                        ResumeId = experience.ResumeId,
                        Job_name = experience.Job_name,
                        Desc = experience.Desc,
                        City = experience.City,
                        Start_d = experience.Start_d,
                        Finish_d = experience.Finish_d,
                        Company_name = experience.Company_name
                    });
                }
                logger.LogInformation($"Successfully retrieved {exps.Count} experience records for ResumeId: {resume_id}");
                return Ok(expresp);

            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while retrieving experiences for ResumeId: {resume_id}: {ex.Message}", ex);
                return BadRequest(new { error = "Не удалось получить данные об опыте" });
            }
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteExperienceById(int exp_id)
        {
            logger.LogDebug($"DeleteExperienceById called with ExperienceId: {exp_id}");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                logger.LogError("UserId is null in DeleteExperienceById.");
                return BadRequest(new { error = "Something went wrong" });
            }
            var exp=await DB.Experience.Include((e)=>e.Resume).FirstOrDefaultAsync(e=>e.Id == exp_id);
            if (exp == null) {
                logger.LogWarning($"Experience with ID: {exp_id} not found.");
                return BadRequest(new { error = "Failed to get data" });
            }
            if (exp.Resume.Job_seekerId != int.Parse(userId))
            {
                logger.LogWarning($"User with ID: {userId} attempted to delete experience not belonging to them (ExperienceId: {exp_id}).");
                return Forbid();
            }
            try
            {
                DB.Experience.Remove(exp);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Successfully deleted experience with ID: {exp_id} for user ID: {userId}");
                return Ok();
            }
            catch(Exception ex) 
            {
                logger.LogError($"Error occurred while deleting experience with ID: {exp_id} for user ID: {userId}: {ex.Message}", ex);
                return BadRequest(new { error = "Не удалось удалить данные об опыте" });
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPatch]
        public async Task<IActionResult> UpdateExperience(ExperienceUpdateRequest request)
        {
            logger.LogDebug($"UpdateExperience called with ExperienceId: {request.Id} by UserId: {HttpContext.User.FindFirstValue("UserId")}");
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                logger.LogError("UserId is null in UpdateExperience.");
                return BadRequest(new { error = "Something went wrong" });
            }
            var exp = await DB.Experience.Include((e) => e.Resume).FirstOrDefaultAsync(e => e.Id == request.Id);
            if (exp == null)
            {
                logger.LogWarning($"Experience with ID: {request.Id} not found.");
                return BadRequest(new { error = "Failed to get data" });
            }
            if (exp.Resume.Job_seekerId != int.Parse(userId))
            {
                return Forbid();
            }
            if (request.Start_d != default(DateOnly))
            {
                exp.Start_d = request.Start_d;
            }
            if (request.Finish_d != default(DateOnly))
            {
                exp.Finish_d = request.Finish_d;
            }
            if (!string.IsNullOrEmpty(request.Company_name))
            {
                exp.Company_name = request.Company_name;
            }
            if (!string.IsNullOrEmpty(request.Job_name))
            {
                exp.Job_name = request.Job_name;
            }
            if (!string.IsNullOrEmpty(request.City))
            {
                exp.City = request.City;
            }
            if (!string.IsNullOrEmpty(request.Desc))
            {
                exp.Desc = request.Desc;
            }
            if (exp.Start_d > exp.Finish_d || exp.Start_d.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
            {
                return BadRequest(new { error = "Wrong period of work" });
            }
            try
            {
                await DB.SaveChangesAsync();
                logger.LogInformation($"Experience with ID: {request.Id} updated successfully by UserId: {userId}.");
                return Ok(new { message = "Опыт обновлен успешно" });
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while updating experience with ID: {request.Id} for user ID: {userId}: {ex.Message}", ex);
                return BadRequest(new { error = "Failed to update data" });
            }
        }

    }
}
