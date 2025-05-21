using JobSeekerService.JSON.ResponseJSON;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Domain.StatusService
{
    public interface IStatusService
    {
        public Task<IEnumerable<StatusResponse>> GetAllStatuses();
    }
}
