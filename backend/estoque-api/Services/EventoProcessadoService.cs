using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using estoque_api.DataContext;
using estoque_api.Models;
using Microsoft.EntityFrameworkCore;

namespace estoque_api.Services
{
    public class EventoProcessadoService
    {
        private readonly AppDbContext _context;
        public EventoProcessadoService(AppDbContext context)
        {
            _context = context;
        }

        public Task Create(EventoProcessado evento)
        {
            _context.Add(evento);
            return Task.CompletedTask;
        }

        public async Task<bool> EventoProcessado(EventoProcessado evento)
        {
            return await _context.EventosProcessados
            .AnyAsync(e =>
                e.EventoId == evento.EventoId &&
                e.EventoType == evento.EventoType
            );
        }
    }
}