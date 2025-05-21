using System.Text.RegularExpressions;
using JobSeekerService.DB.DBContext;
using JobSeekerService.DB.Models;
using JobSeekerService.Domain.Exceptions;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace JobSeekerService.Domain.ResumeService
{
    public class ResumeService:IResumeService
    {
        private Context DB;
        private ILoggerService logger;
        public ResumeService(Context context, ILoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }
        public async Task CreateResume(ResumeRequest request, int userId)
        {

            if (!Regex.IsMatch(request.MobilePhone, @"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)"))
            {
                logger.LogWarning("Invalid mobile phone format provided.");
                throw new WrongDataFormatException("Invalid mobile phone format provided.");
            }
            if (!Regex.IsMatch(request.City, @"^[a-zA-Zа-яА-Я\s\-\+#]+$"))
            {
                logger.LogWarning("Invalid city name format provided.");
                throw new WrongDataFormatException("Invalid city name format provided.");
            }
            if (!Regex.IsMatch(request.JobName, @"^[a-zA-Zа-яА-Я\s-]+$"))
            {
                logger.LogWarning("Invalid job name format provided.");
                throw new WrongDataFormatException("Invalid city name format provided.");
            }
            var st = await DB.Statuses.FirstOrDefaultAsync((s) => s.Id == request.StatusId);
            if (st == null)
            {
                logger.LogWarning("Status ID does not exist.");
                throw new WrongDataFormatException("No such status.");
            }
            try
            {
                await DB.Resumes.AddAsync(new Resume
                {
                    City = request.City,
                    Desc = request.Desc,
                    Mobile_phone = request.MobilePhone,
                    Job_name = request.JobName,
                    Job_seekerId = userId,
                    StatusId = request.StatusId,
                    Salary = ((int)(request.Salary)),
                    Name = request.Name
                });
                await DB.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error while creating resume", ex);
                throw;
            }
        }
        public async Task<IEnumerable<ResponseBriefResume>> GetAllUserBriefResume(int userId)
        {
            try
            {
                var resumes = await (from resume in DB.Resumes
                                     join experience in DB.Experience
                                     on resume.Id equals experience.ResumeId into experienceGroup
                                     where resume.Job_seekerId == userId
                                     select new ResponseBriefResume
                                     {
                                         Id = resume.Id,
                                         Salary = resume.Salary,
                                         JobName = resume.Job_name,
                                         City = resume.City,
                                         Experience = (experienceGroup.Any() ? Math.Round(experienceGroup.Sum(e => (e.Finish_d.ToDateTime(TimeOnly.MinValue) - e.Start_d.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.0) * 2,
                                         MidpointRounding.AwayFromZero) / 2 : 0).ToString()
                                     }).ToListAsync();

                logger.LogInformation($"Found {resumes.Count} resumes for user ID {userId}.");
                return resumes;
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving user resumes.", ex);
                throw;
            }
        }
        public async Task<ResponseResume> GetResumeById(int id)
        {
            try
            {
                var resumes = await (from resume in DB.Resumes
                                     join experience in DB.Experience
                                     on resume.Id equals experience.ResumeId into experienceGroup
                                     join status in DB.Statuses
                                     on resume.StatusId equals status.Id
                                     where resume.Id == id
                                     select new ResponseResume
                                     {
                                         Id = resume.Id,
                                         Salary = resume.Salary,
                                         Job_name = resume.Job_name,
                                         City = resume.City,
                                         Desc = resume.Desc,
                                         Job_seekerId = resume.Job_seekerId,
                                         Mobile_phone = resume.Mobile_phone,
                                         Status = status.Desc,
                                         StatusId = resume.StatusId,
                                         Experience = (experienceGroup.Any() ? Math.Round(experienceGroup.Sum(e => (e.Finish_d.ToDateTime(TimeOnly.MinValue) - e.Start_d.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.0) * 2,
                                         MidpointRounding.AwayFromZero) / 2 : 0).ToString(),
                                         Name = resume.Name
                                     }).ToListAsync();
                var result = resumes.FirstOrDefault();
                if (result == null)
                {
                    logger.LogWarning($"No resume found for ID: {id}");
                    throw new Exception("No such reusme");
                }
                return result;
            }
            catch (Exception ex) {
                logger.LogWarning($"Error: {ex.Message}");
                throw;
            }
        }
        public async  Task<IEnumerable<ResponseBriefResume>> GetAllBriefResume()
        {
            try
            {
                var resumes = await(from resume in DB.Resumes
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
                return resumes;
            }
            catch (Exception ex)
            {
                logger.LogError("Error while retrieving all brief resumes.", ex);
                throw;
            }
        }
        public async Task UpdateResume(UpdateResumeRequest request, int userId)
        {
            try
            {
                var resume = await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == request.Id && r.Job_seekerId == userId);
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
                if (request.Salary != 0) 
                {
                    resume.Salary = request.Salary;
                }
                if (request.StatusId != 0)
                {
                    resume.StatusId = request.StatusId;
                }
                await DB.SaveChangesAsync();
                logger.LogInformation($"Resume ID: {request.Id} successfully updated.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while updating Resume ID: {request.Id}", ex);
                throw;
            }
        }
        public async Task DeleteResume(int resumeId, int userId)
        {
            try
            {
                var resume = await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == resumeId && r.Job_seekerId == userId);
                if (resume == null)
                {
                    logger.LogWarning($"Resume with ID {resumeId} not found for user ID {userId}.");
                    throw new Exception("Resume not found.");
                }
                DB.Resumes.Remove(resume);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Resume ID: {resumeId} successfully deleted.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while deleting Resume ID: {resumeId}", ex);
                throw;
            }
        }
        public async Task AddExperience(ExperienceRequest request, int userId)
        {
            
            try
            {
                var resume = await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == request.ResumeId && r.Job_seekerId == userId);
                if (resume == null)
                {
                    logger.LogWarning($"Resume with ID {request.ResumeId} not found for user ID {userId}.");
                    throw new Exception("Resume not found.");
                }
                if (request.Start_d > request.Finish_d || request.Start_d.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
                {
                    throw new WrongDataFormatException("Wrong data period");
                }
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
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while adding experience for Resume ID {request.ResumeId}: {ex.Message}", ex);
                throw ;
            }
        }
        public async Task<IEnumerable<ExperienceResponse>> GetAllExperienceByResumeId(int resume_id)
        {
            try
            {
                var exps = await DB.Experience.Where((r) => r.ResumeId == resume_id).ToListAsync();
                if (!exps.Any())
                {
                    logger.LogWarning($"No experience found for ResumeId: {resume_id}");
                    return null;
                }
                var expresp = new List<ExperienceResponse>();
                foreach (var experience in exps)
                {
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
                return expresp;

            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while retrieving experiences for ResumeId: {resume_id}: {ex.Message}", ex);
                throw ;
            }
        }
        public async Task DeleteExperienceById(int exp_id, int userId)
        {
            try
            {
                var exp = await DB.Experience.Include((e) => e.Resume).FirstOrDefaultAsync(e => e.Id == exp_id);
                if (exp == null)
                {
                    logger.LogWarning($"Experience with ID: {exp_id} not found.");
                    throw new Exception($"Experience with ID: {exp_id} not found.");
                }
                if (exp.Resume.Job_seekerId != userId)
                {
                    logger.LogWarning($"User with ID: {userId} attempted to delete experience not belonging to them (ExperienceId: {exp_id}).");
                    throw new NoPermissionForUser("User dont have rights to delete this experience");
                }
                DB.Experience.Remove(exp);
                await DB.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while deleting experience with ID: {exp_id} for user ID: {userId}: {ex.Message}", ex);
                throw;
            }
        }
        public async Task UpdateExperience(ExperienceUpdateRequest request, int userId)
        {
            
            try
            {

                var exp = await DB.Experience.Include((e) => e.Resume).FirstOrDefaultAsync(e => e.Id == request.Id);
                if (exp == null)
                {
                    logger.LogWarning($"Experience with ID: {request.Id} not found.");
                    throw new Exception("Failed to get data");
                }
                if (exp.Resume.Job_seekerId != userId)
                {
                    throw new NoPermissionForUser("You dont have permission to change this resume");
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
                    throw new WrongDataFormatException("Wrong period of work");
                }
                await DB.SaveChangesAsync();
                logger.LogInformation($"Experience with ID: {request.Id} updated successfully by UserId: {userId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while updating experience with ID: {request.Id} for user ID: {userId}: {ex.Message}", ex);
                throw;
            }
        }

    }
}
