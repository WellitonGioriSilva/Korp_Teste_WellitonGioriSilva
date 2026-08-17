using System.Text;
using System.Text.Json;
using Contracts.Events;
using RabbitMQ.Client;

namespace estoque_api.RabbitMq;

public class BaixaEstoqueResultadoPublisher : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public BaixaEstoqueResultadoPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task PublishRealizadaAsync(BaixaEstoqueRealizadaEvent evento)
    {
        return PublishAsync(evento, _configuration["RabbitMq:BaixaEstoqueRealizadaRoutingKey"]!);
    }

    public Task PublishFalhouAsync(BaixaEstoqueFalhouEvent evento)
    {
        return PublishAsync(evento, _configuration["RabbitMq:BaixaEstoqueFalhouRoutingKey"]!);
    }

    private async Task PublishAsync<T>(T evento, string routingKey)
    {
        await EnsureChannelAsync();

        var json = JsonSerializer.Serialize(evento);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
        };

        await _channel!.BasicPublishAsync(
            exchange: _configuration["RabbitMq:Exchange"]!,
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

        await _channel.ExchangeDeclareAsync(
            _configuration["RabbitMq:Exchange"]!,
            ExchangeType.Direct,
            durable: true
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}