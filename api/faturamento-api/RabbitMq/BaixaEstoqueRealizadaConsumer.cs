using System.Text;
using System.Text.Json;
using Contracts.Events;
using estoque_api.Exceptions;
using faturamento_api.DataContext;
using faturamento_api.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace faturamento_api.RabbitMq
{
   public class BaixaEstoqueResultadoConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public BaixaEstoqueResultadoConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory
            )
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConfigureRabbitMqAsync();

            var realizadaQueue = _configuration["RabbitMq:BaixaEstoqueRealizadaQueue"]!;
            var falhouQueue = _configuration["RabbitMq:BaixaEstoqueFalhouQueue"]!;

            var consumer = new AsyncEventingBasicConsumer(_channel!);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    if (ea.RoutingKey == _configuration["RabbitMq:BaixaEstoqueRealizadaRoutingKey"])
                    {
                        var evento = JsonSerializer.Deserialize<BaixaEstoqueRealizadaEvent>(json)!;
                        await FecharNotaAsync(evento.NotaFiscalId);
                    }

                    if (ea.RoutingKey == _configuration["RabbitMq:BaixaEstoqueFalhouRoutingKey"])
                    {
                        var evento = JsonSerializer.Deserialize<BaixaEstoqueFalhouEvent>(json)!;
                        await MarcarNotaComErroAsync(evento.NotaFiscalId, evento.Motivo);
                    }

                    await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel!.BasicConsumeAsync(realizadaQueue, autoAck: false, consumer);
            await _channel.BasicConsumeAsync(falhouQueue, autoAck: false, consumer);

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

            var realizadaQueue = _configuration["RabbitMq:BaixaEstoqueRealizadaQueue"]!;
            var falhouQueue = _configuration["RabbitMq:BaixaEstoqueFalhouQueue"]!;

            var realizadaRoutingKey = _configuration["RabbitMq:BaixaEstoqueRealizadaRoutingKey"]!;
            var falhouRoutingKey = _configuration["RabbitMq:BaixaEstoqueFalhouRoutingKey"]!;

            await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);

            await _channel.QueueDeclareAsync(realizadaQueue, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueDeclareAsync(falhouQueue, durable: true, exclusive: false, autoDelete: false);

            await _channel.QueueBindAsync(realizadaQueue, exchange, realizadaRoutingKey);
            await _channel.QueueBindAsync(falhouQueue, exchange, falhouRoutingKey);
        }

        private async Task FecharNotaAsync(int notaFiscalId)
        {
            using var scope = _scopeFactory.CreateScope();
            var notaFiscalService = scope.ServiceProvider.GetRequiredService<NotaFiscalService>();

            await notaFiscalService.UpdateStatus(notaFiscalId, "Fechada");
        }

        private async Task MarcarNotaComErroAsync(int notaFiscalId, string motivo)
        {
            using var scope = _scopeFactory.CreateScope();
            var notaFiscalService = scope.ServiceProvider.GetRequiredService<NotaFiscalService>();
            await notaFiscalService.UpdateStatus(notaFiscalId, "Erro", $"Erro ao processar baixa de estoque: {motivo}");
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
}
