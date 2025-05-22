using EmployerService.Infrastructure.DB.DBContext;
using EmployerService.Infrastructure.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace EmployerService.Domain.Services.ExperienceService
{
    public class ExperienceService:IExperienceService
    {
        private readonly Context DB;
        private readonly ILoggerService logger;

        public ExperienceService(Context context, ILoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }
        public async Task<IEnumerable<ExperienceResponse>> GetAllExperience()
        {
            try
            {
                var expDB = await DB.Experiences.ToListAsync();
                var experiences = new List<ExperienceResponse>();
                foreach (var exp in DB.Experiences)
                {
                    experiences.Add(new ExperienceResponse { Id = exp.Id, Desc = exp.Desc });
                }

                logger.LogInformation($"Successfully fetched {experiences.Count} experiences.");
                return experiences;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching experiences: {ex.Message}");
                throw;
            }
        }
    }
}
