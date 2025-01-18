using DTO;

namespace WS_ClienteProducer.Services.RabbitMQ.Interface
{
    public interface IClientFactory
    {
        Task Client(CostumerDTO costumer);
    }
}
