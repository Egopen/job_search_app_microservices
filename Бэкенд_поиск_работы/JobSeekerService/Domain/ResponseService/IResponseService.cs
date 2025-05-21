using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Domain.ResponseService
{
    public interface IResponseService
    {
        public Task AddResponse(AddResponseToVacancyRequest addResponseJSON,int userId);
        public Task DeleteResponseById(int id, int userId);
        public Task<IEnumerable<ResponseResumeDataResponse>> GetAllResponsesByResumeId(int id, int userId);
    }
}
