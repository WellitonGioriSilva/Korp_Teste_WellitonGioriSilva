using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using faturamento_api.Events;

namespace faturamento_api.Services
{
    public class NotaFiscalSseService
    {
        private readonly ConcurrentDictionary<int, Channel<ImpressaoNotaEvent>> _channels = new();

        public ChannelReader<ImpressaoNotaEvent> Subscribe(int notaFiscalId)
        {
            var channel = Channel.CreateUnbounded<ImpressaoNotaEvent>();

            _channels[notaFiscalId] = channel;

            return channel.Reader;
        }

        public async Task PublishAsync(
            ImpressaoNotaEvent evento)
        {
            if (_channels.TryGetValue(
                evento.NotaFiscalId,
                out var channel))
            {
                await channel.Writer.WriteAsync(evento);
            }
        }

        public void Unsubscribe(int notaFiscalId)
        {
            if (_channels.TryRemove(
                notaFiscalId,
                out var channel))
            {
                channel.Writer.TryComplete();
            }
        }
    }
}