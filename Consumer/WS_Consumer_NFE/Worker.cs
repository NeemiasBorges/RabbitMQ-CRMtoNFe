using DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace WS_Consumer_NFE
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _queuenfe;
        private IConnection _connection;

        public Worker(ILogger<Worker> logger, IConfiguration conf)
        {
            _logger = logger;
            _queuenfe = conf["RabbitMQ:Queue"] ?? "";
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = "localhost" };
                using var connection = await factory.CreateConnectionAsync(stoppingToken);
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: _queuenfe,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                Console.WriteLine($"[*****] Aguardando mensagens na fila {_queuenfe}");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var corpo = ea.Body.ToArray();
                        var mensagem = Encoding.UTF8.GetString(corpo);
                        var costumer = JsonSerializer.Deserialize<CostumerDTO>(mensagem);
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };

                        Console.WriteLine($"[xxxxx] Recebida mensagem da fila nfe: {JsonSerializer.Serialize(costumer, options)}");
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem da fila NFE");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: _queuenfe,
                    autoAck: false,
                    consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no serviço de consumo da fila nfe");
                throw;
            }
        }
    }
}
