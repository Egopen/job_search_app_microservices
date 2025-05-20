using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.RabbitMQClasses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployerService.Domain.Services.RabbitMQ
{
    public class RabbitMQSendStatisticService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILoggerService _logger;

        public RabbitMQSendStatisticService(RabbitMQService rabbitMQService, IServiceProvider serviceProvider, LoggerService logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = "employer_statistic_queue";

            try
            {
                _logger.LogInformation("Starting RabbitMQ statistic background service...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Start generating statistics for queue '{queueName}'.");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
                        var statistic = await GetStatisticsAsync(dbContext);
                        var statisticJson = JsonSerializer.Serialize(statistic);
                        var messageBody = System.Text.Encoding.UTF8.GetBytes(statisticJson);
                        await _rabbitMQService.SendMessageAsync(queueName, messageBody);
                        _logger.LogInformation("Statistic message sent successfully.");
                    }

                    await Task.Delay(30000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in RabbitMQ statistic background service.", ex);
            }
        }

        // Метод для получения статистики из базы данных
        private async Task<VacancyStatisticJSON> GetStatisticsAsync(Context dbContext)
        {
            int total = await dbContext.Vacancies.CountAsync();
            int active = await dbContext.Vacancies
                .CountAsync(v => v.Status.IsActive);
            int inactive = await dbContext.Vacancies
                .CountAsync(v => !v.Status.IsActive);

            _logger.LogInformation($"Statistics generated: Total={total}, Active={active}, Inactive={inactive}");

            return new VacancyStatisticJSON
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                DateOfRecord = DateTime.UtcNow,
            };
        }
    }
}
