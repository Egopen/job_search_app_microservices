using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Domain.Services.ResponseService
{
    public interface IResponseService
    {
        public Task DeleteResponseById(int id,int userId);
        public Task<IEnumerable<ResponseVacancyDataResponse>> GetAllResponsesByVacancyId(int id,int userId);
    }
}
