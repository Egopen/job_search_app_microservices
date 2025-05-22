using EmployerService.Infrastructure.DB.DBContext;
using EmployerService.Infrastructure.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace EmployerService.Domain.Services.StatusService
{
    public class StatusService:IStatusService
    {
        private readonly Context DB;
        private readonly ILoggerService logger;

        public StatusService(Context context, ILoggerService logger)
        {
            DB = context;
            this.logger = logger;
        }
        public async Task<IEnumerable<StatusResponse>> GetAllStatusesAsync()
        {
            try
            {
                var statusDB = await DB.Statuses.ToListAsync();
                var statuses = new List<StatusResponse>();
                foreach (var stat in statusDB)
                {

                    statuses.Add(new StatusResponse { Id = stat.Id, Desc = stat.Desc, Is_active = stat.IsActive });
                }

                logger.LogInformation($"Successfully fetched {statuses.Count} statuses from the database.");
                return statuses;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching statuses: {ex.Message}");
                throw;
            }
        }
    }
}
