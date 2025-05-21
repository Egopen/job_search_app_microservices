using JobSeekerService.DB.DBContext;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace JobSeekerService.Domain.StatusService
{
    public class StatusService:IStatusService
    {
        private readonly Context DB;

        public StatusService(Context context)
        {
            DB = context;
        }
        public async Task<IEnumerable<StatusResponse>> GetAllStatuses()
        {
            try
            {
                var resp = new List<StatusResponse>();
                var statuses = await DB.Statuses.ToListAsync();

                foreach (var status in statuses)
                {
                    resp.Add(new StatusResponse { Id = status.Id, Desc = status.Desc });
                }
                return resp;
            }
            catch (Exception ex) {
                throw;  
            }
        }


    }
}
