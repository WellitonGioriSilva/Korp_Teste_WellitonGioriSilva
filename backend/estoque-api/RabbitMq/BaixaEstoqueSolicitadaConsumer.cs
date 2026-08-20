using System.Text;
using System.Text.Json;
using Contracts.Events;
using estoque_api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace estoque_api.RabbitMq;

public class BaixaEstoqueSolicitadaConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BaixaEstoqueResultadoPublisher _resultadoPublisher;
    private IConnection? _connection;
    private IChannel? _channel;

    public BaixaEstoqueSolicitadaConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        BaixaEstoqueResultadoPublisher resultadoPublisher
    )
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _resultadoPublisher = resultadoPublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConfigureRabbitMqAsync();

        var queue = _configuration["RabbitMq:BaixaEstoqueSolicitadaQueue"]!;

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            BaixaEstoqueSolicitadaEvent? evento = null;

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                evento = JsonSerializer.Deserialize<BaixaEstoqueSolicitadaEvent>(json)!;

                await BaixarEstoqueAsync(evento);

                await _resultadoPublisher.PublishRealizadaAsync(new BaixaEstoqueRealizadaEvent
                {
                    NotaFiscalId = Convert.ToInt32(evento.EventoId),
                    DataProcessamento = DateTime.UtcNow
                });

                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                if (evento is not null)
                {
                    await _resultadoPublisher.PublishFalhouAsync(new BaixaEstoqueFalhouEvent
                    {
                        NotaFiscalId = Convert.ToInt32(evento.EventoId),
                        Motivo = ex.Message,
                        DataProcessamento = DateTime.UtcNow
                    });
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
        };

        await _channel!.BasicConsumeAsync(queue, autoAck: false, consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConfigureRabbitMqAsync()
    {
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

    private async Task BaixarEstoqueAsync(BaixaEstoqueSolicitadaEvent evento)
    {
        using var scope = _scopeFactory.CreateScope();
        var produtoService = scope.ServiceProvider.GetRequiredService<ProdutoService>();

        await produtoService.BaixarEstoque(evento);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
