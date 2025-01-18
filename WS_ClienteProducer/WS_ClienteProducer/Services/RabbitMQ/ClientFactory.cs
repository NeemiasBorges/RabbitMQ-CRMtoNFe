using DTO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WS_ClienteProducer.Services.RabbitMQ.Interface;

namespace WS_ClienteProducer.Services.RabbitMQ
{
    public class ClientFactory : IClientFactory
    {
        private readonly string _queueCrm;
        private readonly string _queueNfe;
        private readonly string _routingKeyCrm;
        private readonly string _routingKeyNfe;
        private readonly Random _random;

        public ClientFactory(IConfiguration conf)
        {
            _queueCrm = conf["RabbitMQ:QueueCrm"] ?? "fila.crm";
            _queueNfe = conf["RabbitMQ:QueueNfe"] ?? "fila.nfe";
            _routingKeyCrm = conf["RabbitMQ:RoutingKeyCrm"] ?? "rk.crm";
            _routingKeyNfe = conf["RabbitMQ:RoutingKeyNfe"] ?? "rk.nfe";
            _random = new Random();
        }

        public async Task Client(CostumerDTO costumer)
        {
            var   factory        = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel    = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _queueCrm,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueueDeclareAsync(
                queue: _queueNfe,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            bool useCrm               = _random.Next(2) == 0;
            string selectedQueue      = useCrm ? _queueCrm : _queueNfe;
            string selectedRoutingKey = useCrm ? _routingKeyCrm : _routingKeyNfe;

            var mensagem = JsonSerializer.Serialize(costumer);
            var corpo    = Encoding.UTF8.GetBytes(mensagem);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: selectedQueue,
                body: corpo);
        }
    }
}