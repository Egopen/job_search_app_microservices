using JobSeekerService.DB.DBContext;
using JobSeekerService.Features.Logger;
using JobSeekerService.JSON.RabbitMQClasses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobSeekerService.Domain.RabbitMQ
{
    public class RabbitMQSendResumeStatisticService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; 
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILoggerService _logger;

        public RabbitMQSendResumeStatisticService(RabbitMQService rabbitMQService, IServiceProvider serviceProvider, LoggerService logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = "seeker_statistic_queue";

            try
            {
                _logger.LogInformation("Starting RabbitMQ resume statistic background service...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Start sending resume statistics to queue '{queueName}'.");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
                        var statistics = await GetResumeStatistics(dbContext);
                        var jsonMessage = JsonSerializer.Serialize(statistics);
                        var messageBody = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
                        await _rabbitMQService.SendMessageAsync(queueName, messageBody);
                    }

                    await Task.Delay(30000, stoppingToken); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in RabbitMQ resume statistic background service.", ex);
            }
        }

        private async Task<ResumeStatisticJSON> GetResumeStatistics(Context dbContext)
        {
            var totalResumes = await dbContext.Resumes.CountAsync();
            var activeResumes = await dbContext.Resumes.CountAsync(r => r.Status.Is_active);
            var inactiveResumes = totalResumes - activeResumes;
            var recordTime = DateTime.UtcNow;

            return new ResumeStatisticJSON
            {
                Total = totalResumes,
                Active = activeResumes,
                Inactive = inactiveResumes,
                DateOfRecord = recordTime,
            };
        }
    }
}
