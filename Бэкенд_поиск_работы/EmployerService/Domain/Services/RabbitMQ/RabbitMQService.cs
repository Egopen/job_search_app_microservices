using EmployerService.Infrastructure.Features.Logger;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EmployerService.Domain.Services.RabbitMQ
{
    public class RabbitMQService
    {
        private static readonly LoggerService _logger = new LoggerService();
        private IChannel? _channel;

        private async Task Initialize()
        {
            string hostName = "rabbitmq";
            string userName = "user";
            string password = "password";

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            try
            {
                _logger.LogInformation("Starting RabbitMQ initialization...");

                var connection = await factory.CreateConnectionAsync();
                _logger.LogInformation("Connection to RabbitMQ established successfully.");

                _channel = await connection.CreateChannelAsync();
                _logger.LogInformation("Channel created successfully.");
                var queues = new[] { "seeker_response_queue", "employer_statistic_queue", "seeker_statistic_queue" };

                foreach (var queue in queues)
                {
                    await CreateQueue(_channel, queue);
                }

                _logger.LogInformation("RabbitMQ queues initialized successfully.");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError("RabbitMQ broker is unreachable. Check your RabbitMQ configuration.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Unhandled exception occurred during RabbitMQ setup.", ex);
            }
        }

        private static async Task CreateQueue(IChannel channel, string queueName)
        {
            try
            {
                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _logger.LogInformation($"Queue '{queueName}' created successfully.");
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogWarning($"Queue creation for '{queueName}' failed. It might already exist.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception occurred while creating queue '{queueName}'.", ex);
            }
        }

        // Метод для отправки сообщения
        public async Task SendMessageAsync(string queueName, byte[] body)
        {
            try
            {
                if (_channel == null)
                {
                    await Initialize();
                    if (_channel == null)
                    {
                        _logger.LogError("RabbitMQ channel is still not initialized.");
                        return;
                    }
                }

                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
                _logger.LogInformation($"Message sent to queue '{queueName}' successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while sending message to RabbitMQ.", ex);
            }
        }

        public async Task<List<string>> ReceiveAllMessagesAsync(string queueName)
        {
            var messages = new List<string>();

            try
            {
                if (_channel == null)
                {
                    await Initialize();
                    if (_channel == null)
                    {
                        _logger.LogError("RabbitMQ channel is still not initialized.");
                        return messages;
                    }
                }

                _logger.LogInformation($"Start receiving messages from queue '{queueName}'...");

                var consumer = new AsyncDefaultBasicConsumer(_channel);
                bool queueHasMessages = true;

                while (queueHasMessages)
                {
                    try
                    {
                        var result = await _channel.BasicGetAsync(queue: queueName, autoAck: false);

                        if (result == null)
                        {
                            queueHasMessages = false;
                        }
                        else
                        {
                            var body = result.Body.ToArray();
                            var message = System.Text.Encoding.UTF8.GetString(body);
                            messages.Add(message);

                            _logger.LogInformation($"Message received: {message}");

                            await _channel.BasicAckAsync(result.DeliveryTag, multiple: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error while retrieving messages from queue '{queueName}'.", ex);
                        queueHasMessages = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while receiving messages from RabbitMQ.", ex);
            }

            _logger.LogInformation($"Finished receiving messages from queue '{queueName}'. Total messages retrieved: {messages.Count}");
            return messages;
        }

    }
}
