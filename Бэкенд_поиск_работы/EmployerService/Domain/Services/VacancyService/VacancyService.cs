using EmployerService.DB.DBContext;
using EmployerService.DB.Models;
using EmployerService.Features.Logger;
using EmployerService.JSON.RequestJSON;
using EmployerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace EmployerService.Domain.Services.VacancyService
{
    public class VacancyService : IVacancyService
    {
        private readonly Context DB;
        private readonly ILoggerService logger;

        public VacancyService(Context context, ILoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }
        public async Task<IEnumerable<BriefVacancyResponse>> GetAllBriefVacancy()
        {
            try
            {
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies successfully.");
                return vac;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching brief vacancies: {ex.Message}");
                throw;
            }
        }
        public async Task<VacancyResponse> GetVacancyById(int id)
        {
            try
            {
                var vacancy = await (from vc in DB.Vacancies
                                     join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                     join status in DB.Statuses on vc.StatusId equals status.Id
                                     where status.IsActive == true && vc.Id == id
                                     select new VacancyResponse
                                     {
                                         Id = vc.Id,
                                         Job_name = vc.Job_name,
                                         Experience_desc = exp.Desc,
                                         Description = vc.Description,
                                         Status_description = status.Desc,
                                         Experience_id = exp.Id,
                                         Status_id = vc.StatusId,
                                         Employer_id = vc.EmployerId,
                                         City = vc.City
                                     }).FirstOrDefaultAsync();

                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {id} not found.");
                    throw new Exception("No vacancy found");
                }

                logger.LogInformation($"Fetched vacancy details for ID {id} successfully.");
                return vacancy;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching vacancy by ID {id}: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<BriefVacancyResponse>> GetAllEmployerBriefVacancy(int employer_id)
        {
            try
            {
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true && vc.EmployerId == employer_id
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies for employer ID {employer_id} successfully.");
                return vac;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching vacancies for employer ID {employer_id}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<BriefVacancyResponse>> GetAllOwnBriefVacancy(int userId)
        {
            try
            {
                var vac = await (from vc in DB.Vacancies
                                 join exp in DB.Experiences on vc.ExperienceId equals exp.Id
                                 join status in DB.Statuses on vc.StatusId equals status.Id
                                 where status.IsActive == true && vc.EmployerId == userId
                                 select new BriefVacancyResponse
                                 {
                                     Id = vc.Id,
                                     Job_name = vc.Job_name,
                                     Experience_desc = exp.Desc,
                                     Experience_id = exp.Id,
                                     Status_id = vc.StatusId,
                                     Employer_id = vc.EmployerId,
                                     City = vc.City
                                 }).ToListAsync();
                logger.LogInformation($"Fetched {vac.Count} brief vacancies for authenticated employer successfully.");
                return vac;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while fetching employer's own vacancies: {ex.Message}");
                throw;
            }
        }
        public async Task AddVacancy(AddVacancyRequest request, int userId)
        {
            try
            {
                var vacancy = new Vacancy
                {
                    City = request.City,
                    Description = request.Description,
                    EmployerId = userId,
                    ExperienceId = request.Experience_id,
                    Job_name = request.Job_name,
                    StatusId = request.Status_id
                };
                await DB.Vacancies.AddAsync(vacancy);
                await DB.SaveChangesAsync();
                logger.LogInformation($"New vacancy added successfully by employer ID {userId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while adding new vacancy: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteVacancy(int vacancy_id, int userId)
        {
            try
            {
                var vacancy = await DB.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancy_id && v.EmployerId ==userId);
                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {vacancy_id} not found for deletion.");
                    throw new Exception("Vacancy not found");
                }
                DB.Vacancies.Remove(vacancy);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Vacancy with ID {vacancy_id} deleted successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while deleting vacancy ID {vacancy_id}: {ex.Message}");
                throw;
            }
        }
        public async Task UpdateVacancy(UpdateVacancyRequest request,int userId)
        {
            try
            {
                var vacancy = await DB.Vacancies.FirstOrDefaultAsync(v => v.Id == request.Id && v.EmployerId == userId);
                if (vacancy == null)
                {
                    logger.LogWarning($"Vacancy with ID {request.Id} not found for update.");
                    throw new Exception("Vacancy not found");
                }

                if (!string.IsNullOrEmpty(request.Job_name)) vacancy.Job_name = request.Job_name;
                if (!string.IsNullOrEmpty(request.Description)) vacancy.Description = request.Description;
                if (!string.IsNullOrEmpty(request.City)) vacancy.City = request.City;
                if (request.Status_id > 0) vacancy.StatusId = request.Status_id;
                if (request.Experience_id > 0) vacancy.ExperienceId = request.Experience_id;

                await DB.SaveChangesAsync();
                logger.LogInformation($"Vacancy with ID {request.Id} updated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while updating vacancy ID {request.Id}: {ex.Message}");
                throw;
            }
        }
    }
}
