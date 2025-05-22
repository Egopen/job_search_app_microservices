using System.Text.Json;
using JobSeekerService.Domain.RabbitMQ;
using JobSeekerService.Infrastructure.DB.DBContext;
using JobSeekerService.Infrastructure.Features.Logger;
using JobSeekerService.JSON.RequestJSON;
using JobSeekerService.JSON.ResponseJSON;
using Microsoft.EntityFrameworkCore;

namespace JobSeekerService.Domain.ResponseService
{
    public class ResponseService:IResponseService
    {
        private Context DB;
        private ILoggerService logger;
        private RabbitMQService rabbitMQService;
        public ResponseService(Context context, ILoggerService logger, RabbitMQService rabbitMQService)
        {
            DB = context;
            this.logger = logger;
            this.rabbitMQService = rabbitMQService;
        }
        public async Task AddResponse(AddResponseToVacancyRequest addResponseJSON, int userId)
        {
            try
            {
                if (await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == addResponseJSON.Resume_id && r.Job_seekerId == userId) == null)
                {
                    logger.LogWarning($"Something went wrong");
                    throw new Exception("Something went wrong") ;
                }
                await DB.Responses.AddAsync(new DB.Models.Response { ResumeId = addResponseJSON.Resume_id, Vacancy_id = addResponseJSON.Vacancy_id });
                await DB.SaveChangesAsync();
                var mes = JsonSerializer.Serialize(addResponseJSON);
                var body = System.Text.Encoding.UTF8.GetBytes(mes);
                await rabbitMQService.SendMessageAsync("seeker_response_queue", body);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task DeleteResponseById(int id, int userId)
        {
            try
            {
                var res = await DB.Responses.FirstOrDefaultAsync((r) => r.Id == id);
                if ( res == null || await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == res.ResumeId && r.Job_seekerId == userId) == null)
                {
                    logger.LogWarning("Something went wrong");
                    throw new Exception("Something went wrong");
                }
                DB.Responses.Remove(res);
                await DB.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ResponseResumeDataResponse>> GetAllResponsesByResumeId(int id, int userId)
        {
            try
            {
                if (await DB.Resumes.FirstOrDefaultAsync((r) => r.Id == id && r.Job_seekerId == userId) == null)
                {
                    logger.LogWarning("Something went wrong");
                    throw new Exception("Something went wrong");
                }
                var res = await (from r in DB.Responses
                                 where r.ResumeId == id
                                 select new ResponseResumeDataResponse
                                 {
                                     Id = r.Id,
                                     Resume_id = r.ResumeId,
                                     Vacancy_id = r.Vacancy_id
                                 }).ToListAsync();
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
