using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Domain.ResumeService
{
    public interface IResumeService
    {
        public Task CreateResume(ResumeRequest request, int userId);
        public Task<IEnumerable<ResponseBriefResume>> GetAllUserBriefResume(int userId);
        public Task<ResponseResume> GetResumeById(int id);
        public Task<IEnumerable<ResponseBriefResume>> GetAllBriefResume();
        public Task UpdateResume(UpdateResumeRequest request, int userId);
        public Task DeleteResume(int resumeId, int userId);
        public Task AddExperience(ExperienceRequest request, int userId);
        public Task<IEnumerable<ExperienceResponse>> GetAllExperienceByResumeId(int resume_id);
        public Task DeleteExperienceById(int exp_id, int userId);
        public Task UpdateExperience(ExperienceUpdateRequest request, int userId);


    }
}
