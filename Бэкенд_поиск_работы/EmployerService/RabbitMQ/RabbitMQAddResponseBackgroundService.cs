﻿using EmployerService.DB.DBContext;
using EmployerService.Features.Logger;
using EmployerService.JSON.RabbitMQClasses;
using Microsoft.EntityFrameworkCore;
using RabbitMQInitializer;
using System.Text.Json;

namespace EmployerService.RabbitMQ
{
    public class RabbitMQAddResponseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // Используем IServiceProvider для создания scope
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILoggerService _logger;

        public RabbitMQAddResponseBackgroundService(RabbitMQService rabbitMQService, IServiceProvider serviceProvider, LoggerService logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider; // получаем провайдер сервисов
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = "seeker_response_queue";

            _logger.LogInformation("Starting RabbitMQ background service...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Checking for messages in queue '{queueName}'.");

                    // Получаем все сообщения из очереди
                    var messages = await _rabbitMQService.ReceiveAllMessagesAsync(queueName);

                    if (messages.Count == 0)
                    {
                        _logger.LogInformation("No messages found in the queue.");
                    }
                    else
                    {
                        foreach (var message in messages)
                        {
                            try
                            {
                                // Пытаемся сохранить сообщение в базе данных
                                _logger.LogInformation("Processing message: " + message);

                                // Используем scope для создания экземпляра dbContext
                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var dbContext = scope.ServiceProvider.GetRequiredService<Context>(); 
                                    await SaveMessageToDatabaseAsync(dbContext, message);
                                }

                                _logger.LogInformation("Message processed and saved to database successfully.");
                            }
                            catch (JsonException jsonEx)
                            {
                                // Логируем ошибку сериализации JSON
                                _logger.LogError("Failed to deserialize message: " + message, jsonEx);
                            }
                            catch (DbUpdateException dbEx)
                            {
                                // Логируем ошибки сохранения в базу данных
                                _logger.LogError("Failed to save message to the database.", dbEx);
                            }
                            catch (Exception ex)
                            {
                                // Общая ошибка обработки одного сообщения
                                _logger.LogError("Unhandled exception while processing message: " + message, ex);
                            }
                        }
                    }

                    // Ждем перед следующей итерацией
                    _logger.LogInformation("Waiting for the next iteration...");
                    await Task.Delay(15000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Фоновый сервис остановлен
                    _logger.LogWarning("RabbitMQ background service was cancelled.");
                }
                catch (Exception ex)
                {
                    // Общая ошибка фона
                    _logger.LogFatal("Unhandled exception in RabbitMQ background service.", ex);
                }
            }

            _logger.LogInformation("RabbitMQ background service stopped.");
        }

        private async Task SaveMessageToDatabaseAsync(Context dbContext, string message)
        {
            _logger.LogInformation("Attempting to save message to database...");

            var jsonMessage = JsonSerializer.Deserialize<AddResponse>(message);

            if (jsonMessage == null)
            {
                _logger.LogWarning("Deserialized message is null. Skipping.");
                return;
            }

            var response = new DB.Models.Response
            {
                Resume_id = jsonMessage.Resume_id,
                VacancyId = jsonMessage.Vacancy_id
            };

            try
            {
                await dbContext.Responses.AddAsync(response);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Message saved to database successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError("Database update failed while saving message.", dbEx);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled exception while saving message to database.", ex);
                throw;
            }
        }
    }
}
