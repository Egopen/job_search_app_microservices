using JobSeekerService.DB.DBContext;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using JobSeekerService.DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using JobSeekerService.Domain.ResumeService;
using JobSeekerService.Domain.Exceptions;
using JobSeekerService.Infrastructure.Features.Logger;

namespace JobSeekerService.Controllers
{
    [Route("JobSeeker/[controller]/[action]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private LoggerService logger;
        private IResumeService resumeService;
        public ResumeController(LoggerService logger,IResumeService resumeService)
        {
            this.resumeService = resumeService;
            this.logger = logger;
        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateResume(ResumeRequest request)
        {
            try
            {
                logger.LogDebug("CreateResume method called.");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.CreateResume(request,int.Parse(userId));
                logger.LogInformation("Resume created successfully for user ID " + userId);
                return Ok();
            }
            catch (WrongDataFormatException ex)
            {
                logger.LogError($"Wrong data provided by user: {ex.Message}");
                return BadRequest(new { error = ex.Message });
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
            try
            {
                logger.LogDebug("GetAllUserBriefResume method called.");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                var resumes = await resumeService.GetAllUserBriefResume(int.Parse(userId));
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
            try
            {
                logger.LogDebug($"GetResumeById method called with ID: {id}");
                var resume = await resumeService.GetResumeById(id);
                if (resume == null)
                {
                    logger.LogWarning($"No resume found for ID: {id}");
                    return BadRequest(new { error = "No such reusme" });
                }
                logger.LogInformation($"Resume found for ID: {id}");
                return Ok(resume);
            }
            catch(Exception ex)
            {
                logger.LogError("Error while retrieving resume.", ex);
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBriefResume()
        {
            
            try
            {
                logger.LogDebug("GetAllBriefResume method called.");
                var resumes = await resumeService.GetAllBriefResume();
                logger.LogInformation("Successfully generated response for all brief resumes.");
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving all brief resumes.", ex);
                return StatusCode(500);
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPatch]
        public async Task<IActionResult> UpdateResume(UpdateResumeRequest request)
        {
            
            try
            {
                logger.LogDebug($"UpdateResume method called for Resume ID: {request.Id}");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.UpdateResume(request,int.Parse(userId));
                logger.LogInformation($"Resume ID: {request.Id} successfully updated.");
                return Ok();
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while updating Resume ID: {request.Id}", ex);
                return StatusCode(500);
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteResume(int resumeId)
        {
            try
            {
                logger.LogDebug($"DeleteResume method called for Resume ID: {resumeId}");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.DeleteResume(resumeId,int.Parse(userId));
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
            
            try
            {
                logger.LogDebug("AddExperience method called");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogWarning("User ID not found in the token.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.AddExperience(request,int.Parse(userId));
                logger.LogInformation($"Experience for Resume ID {request.ResumeId} successfully added.");
                return Ok();
            }
            catch(WrongDataFormatException ex)
            {
                logger.LogError($"Error occurred while adding experience for Resume ID {request.ResumeId}: {ex.Message}", ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while adding experience for Resume ID {request.ResumeId}: {ex.Message}", ex);
                return BadRequest(new { error = "Не удалось добавить опыт" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExperienceByResumeId(int resume_id)
        {
            try
            {
                logger.LogDebug($"GetAllExperienceByResumeId called with ResumeId: {resume_id}");

                var expresp =await resumeService.GetAllExperienceByResumeId(resume_id);
                logger.LogInformation($"Successfully retrieved experience records for ResumeId: {resume_id}");
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
            
            try
            {
                logger.LogDebug($"DeleteExperienceById called with ExperienceId: {exp_id}");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogError("UserId is null in DeleteExperienceById.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.DeleteExperienceById(exp_id,int.Parse(userId));
                logger.LogInformation($"Successfully deleted experience with ID: {exp_id} for user ID: {userId}");
                return Ok();
            }
            catch(Exception ex) 
            {
                logger.LogError($"Error occurred while deleting experience with ID: {exp_id}: {ex.Message}", ex);
                return BadRequest(new { error = "Не удалось удалить данные об опыте" });
            }

        }

        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPatch]
        public async Task<IActionResult> UpdateExperience(ExperienceUpdateRequest request)
        {
            try
            {
                logger.LogDebug($"UpdateExperience called with ExperienceId: {request.Id} by UserId: {HttpContext.User.FindFirstValue("UserId")}");
                var userId = HttpContext.User.FindFirstValue("UserId");
                if (userId == null)
                {
                    logger.LogError("UserId is null in UpdateExperience.");
                    return BadRequest(new { error = "Something went wrong" });
                }
                await resumeService.UpdateExperience(request,int.Parse(userId));
                logger.LogInformation($"Experience with ID: {request.Id} updated successfully by UserId: {userId}.");
                return Ok(new { message = "Опыт обновлен успешно" });
            }
            catch (WrongDataFormatException ex)
            {
                logger.LogError($"Error occurred while updating experience with ID: {request.ResumeId}: {ex.Message}", ex);
                return BadRequest(new { error = ex.Message });
            }
            catch(NoPermissionForUser ex)
            {
                logger.LogError($"Wrong data format for experience with ID: {request.ResumeId}: {ex.Message}", ex);
                return BadRequest(new { error = "You have no rights to update this experience"});
            }
            catch (Exception ex) {
                logger.LogError($"Error occurred while updating experience with ID: {request.Id}: {ex.Message}", ex);
                return BadRequest(new { error = "Failed to update data" });
            }
        }

    }
}
