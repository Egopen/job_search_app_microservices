using Microsoft.EntityFrameworkCore;
using RabbitMQInitializer;
using StatisticService.DB.DBContext;
using StatisticService.JSON.RabbitMQClasses;
using StatisticService.DB.Models;
using System.Text.Json;

namespace StatisticService.RabbitMQ
{
    public class RabbitMQConsumeStatisticService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<RabbitMQConsumeStatisticService> _logger;

        public RabbitMQConsumeStatisticService(RabbitMQService rabbitMQService, IServiceProvider serviceProvider, ILogger<RabbitMQConsumeStatisticService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var employerQueue = "employer_statistic_queue";
            var seekerQueue = "seeker_statistic_queue";

            _logger.LogInformation("Starting RabbitMQ consume statistic service...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();

                        // Получить сообщения из очередей
                        var employerMessages = await _rabbitMQService.ReceiveAllMessagesAsync(employerQueue);
                        var seekerMessages = await _rabbitMQService.ReceiveAllMessagesAsync(seekerQueue);

                        // Обработать сообщения
                        foreach (var message in employerMessages)
                        {
                            await ProcessVacancyStatisticMessage(dbContext, message);
                        }

                        foreach (var message in seekerMessages)
                        {
                            await ProcessResumeStatisticMessage(dbContext, message);
                        }

                        await dbContext.SaveChangesAsync(stoppingToken); 
                    }

                    await Task.Delay(15000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in RabbitMQ consume statistic service: {Error}", ex);
                }
            }
        }

        private async Task ProcessVacancyStatisticMessage(Context dbContext, string message)
        {
            try
            {
                var statistic = JsonSerializer.Deserialize<VacancyStatisticJSON>(message);

                if (statistic != null)
                {
                    var activePart = ((double)statistic.Active / statistic.Total * 100).ToString("0.00") + "%";
                    var inactivePart = ((double)statistic.Inactive / statistic.Total * 100).ToString("0.00") + "%";

                    var record = new VacancyStatistic
                    {
                        Total = statistic.Total,
                        Active = statistic.Active,
                        Active_part = activePart,
                        Inactive = statistic.Inactive,
                        Inactive_part = inactivePart,
                        Date_of_record = statistic.DateOfRecord,
                    };

                    await dbContext.VacancyStatistics.AddAsync(record);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to process vacancy statistic message: {Error}", ex);
            }
        }

        private async Task ProcessResumeStatisticMessage(Context dbContext, string message)
        {
            try
            {
                var statistic = JsonSerializer.Deserialize<ResumeStatisticJSON>(message);

                if (statistic != null)
                {
                    var activePart = ((double)statistic.Active / statistic.Total * 100).ToString("0.00") + "%";
                    var inactivePart = ((double)statistic.Inactive / statistic.Total * 100).ToString("0.00") + "%";

                    var record = new ResumeStatistic
                    {
                        Total = statistic.Total,
                        Active = statistic.Active,
                        Active_part = activePart,
                        Inactive = statistic.Inactive,
                        Inactive_part = inactivePart,
                        Date_of_record = statistic.DateOfRecord,
                    };

                    await dbContext.ResumeStatistics.AddAsync(record);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to process resume statistic message: {Error}", ex);
            }
        }
    }
}
