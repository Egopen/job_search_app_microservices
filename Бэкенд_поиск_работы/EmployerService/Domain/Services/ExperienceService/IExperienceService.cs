using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Domain.Services.ExperienceService
{
    public interface IExperienceService
    {
        public Task<IEnumerable<ExperienceResponse>> GetAllExperience();
    }
}
