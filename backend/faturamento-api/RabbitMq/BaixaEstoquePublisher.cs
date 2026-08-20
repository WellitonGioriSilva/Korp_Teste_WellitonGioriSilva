using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Contracts.Events;
using RabbitMQ.Client;

namespace faturamento_api.RabbitMq
{
    public class BaixaEstoquePublisher : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public BaixaEstoquePublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishAsync(BaixaEstoqueSolicitadaEvent evento)
        {
            await EnsureChannelAsync();

            var exchange = _configuration["RabbitMq:Exchange"]!;
            var routingKey = _configuration["RabbitMq:BaixaEstoqueSolicitadaRoutingKey"]!;

            var json = JsonSerializer.Serialize(evento);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel!.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }

        private async Task EnsureChannelAsync()
        {
            if (_channel is not null && _channel.IsOpen)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:HostName"],
                UserName = _configuration["RabbitMq:UserName"],
                Password = _configuration["RabbitMq:Password"]
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var exchange = _configuration["RabbitMq:Exchange"]!;
            var queue = _configuration["RabbitMq:BaixaEstoqueSolicitadaQueue"]!;
            var routingKey = _configuration["RabbitMq:BaixaEstoqueSolicitadaRoutingKey"]!;

            await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);
            await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(queue, exchange, routingKey);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
                await _channel.DisposeAsync();

            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}