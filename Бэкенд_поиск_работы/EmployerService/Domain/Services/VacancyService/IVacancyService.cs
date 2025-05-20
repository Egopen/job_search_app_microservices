using EmployerService.JSON.RequestJSON;
using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Domain.Services.VacancyService
{
    public interface IVacancyService
    {
        public Task<IEnumerable<BriefVacancyResponse>> GetAllBriefVacancy();
        public Task<VacancyResponse> GetVacancyById(int id);
        public Task<IEnumerable<BriefVacancyResponse>> GetAllEmployerBriefVacancy(int employer_id);
        public Task<IEnumerable<BriefVacancyResponse>> GetAllOwnBriefVacancy(int userId);
        public Task AddVacancy(AddVacancyRequest request,int userId);
        public Task DeleteVacancy(int vacancy_id, int userId);
        public Task UpdateVacancy(UpdateVacancyRequest request,int userId);
    }
}
