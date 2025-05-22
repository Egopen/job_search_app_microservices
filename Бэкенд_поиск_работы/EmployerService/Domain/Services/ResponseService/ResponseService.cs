using EmployerService.Infrastructure.DB.DBContext;
using EmployerService.Infrastructure.Features.Logger;
using EmployerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace EmployerService.Domain.Services.ResponseService
{
    public class ResponseService:IResponseService
    {
        private Context DB;
        private ILoggerService logger;
        public ResponseService(Context context, ILoggerService logger) 
        {
            DB = context;
            this.logger = logger;
        }
        public async Task DeleteResponseById(int id,int userId)
        {
            logger.LogInformation($"Request received to delete response with ID: {id}.");
            try
            {
                var res = await DB.Responses.FirstOrDefaultAsync((r) => r.Id == id);
                if ( res == null ||
                    await DB.Vacancies.FirstOrDefaultAsync((v) => v.Id == res.VacancyId && v.EmployerId == userId) == null)
                {
                    logger.LogWarning($"Delete operation failed. Either User ID is null, response not found, or access denied for User ID: {userId}.");
                    throw new Exception("Somethin went wrong");
                }

                DB.Responses.Remove(res);
                await DB.SaveChangesAsync();
                logger.LogInformation($"Response with ID {id} successfully deleted by User ID: {userId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while deleting response with ID {id}: {ex.Message}");
                throw;
            }
        }
        public async  Task<IEnumerable<ResponseVacancyDataResponse>> GetAllResponsesByVacancyId(int id,int userId)
        {
            try
            {
                if (
                    await DB.Vacancies.FirstOrDefaultAsync((v) => v.Id == id && v.EmployerId == userId) == null)
                {
                    logger.LogWarning($"Fetch operation failed. Either User ID is null or access denied for Vacancy ID: {id} and User ID: {userId}.");
                    throw new Exception("Somethin went wrong");
                }

                var res = await (from r in DB.Responses
                                 where r.VacancyId == id
                                 select new ResponseVacancyDataResponse
                                 {
                                     Id = r.Id,
                                     Resume_id = r.Resume_id,
                                     Vacancy_id = r.VacancyId
                                 }).ToListAsync();

                logger.LogInformation($"Successfully fetched {res.Count} responses for Vacancy ID: {id} by User ID: {userId}.");
                return res;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while fetching responses for Vacancy ID {id}: {ex.Message}");
                throw;
            }
        }
    }
}
