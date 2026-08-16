using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using faturamento_api.DataContext;
using faturamento_api.DTOs;
using faturamento_api.Models;

namespace faturamento_api.Services
{
    public class NotaFiscalService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public NotaFiscalService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<NotaFiscal> Create(NotaFiscalCreateDTO notaFiscalCreateDTO)
        {
            var notaFiscal = _mapper.Map<NotaFiscal>(notaFiscalCreateDTO);
            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();
            return notaFiscal;
        }

        public async Task<IEnumerable<NotaFiscal>> FindAll()
        {
            var notasFiscais = _context.NotasFiscais.ToList();
            return notasFiscais;
        }

        public async Task<NotaFiscal> FindOne(int id)
        {
            var notaFiscal = await _context.NotasFiscais.FindAsync(id);
            if (notaFiscal == null)
            {
                throw new Exception("Nota fiscal não encontrada.");
            }
            return notaFiscal;
        }
    
        public async Task<NotaFiscal> ImpressaoNotaFiscal(int id)
        {
            // Chama aqui o RabbitMQ para atualizar estoque produto
            var notaFiscal = FindOne(id).Result;
            notaFiscal.Status = "Fechada";
            await _context.SaveChangesAsync();
            return notaFiscal;
        }
    }
}