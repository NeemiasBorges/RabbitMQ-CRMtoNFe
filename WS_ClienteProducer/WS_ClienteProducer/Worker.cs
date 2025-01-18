using DTO;
using Microsoft.Extensions.ObjectPool;
using System.Text.Json;
using WS_ClienteProducer.Services.RabbitMQ.Interface;

public class Worker : BackgroundService
{
    private readonly ObjectPool<CostumerDTO> _customerPool;
    private readonly ILogger<Worker> _logger;
    private readonly IClientFactory _clientFactory;

    public Worker(
        ILogger<Worker> logger,
        IClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _customerPool = ObjectPool.Create<CostumerDTO>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            CostumerDTO? customer = null;
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    customer = _customerPool.Get();
                    customer = customer.CreateSortCostumer();

                    await _clientFactory.Client(customer);
                    _logger.LogInformation("Enviado: {Customer}",
                        JsonSerializer.Serialize(customer, serializerOptions));
                    customer.Dispose();
                }

                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao processar cliente");
            }
            finally
            {
                if (customer != null)
                {
                    _customerPool.Return(customer);
                }
            }
        }
    }
}