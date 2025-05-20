using EmployerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace EmployerService.Domain.Services.StatusService
{
    public interface IStatusService
    {
        public Task<IEnumerable<StatusResponse>> GetAllStatusesAsync();
    }
}
