using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Events;
using estoque_api.Exceptions;
using faturamento_api.DataContext;
using faturamento_api.DTOs;
using faturamento_api.Models;
using faturamento_api.RabbitMq;
using Microsoft.EntityFrameworkCore;

namespace faturamento_api.Services
{
    public class NotaFiscalService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly BaixaEstoquePublisher _baixaEstoquePublisher;
        private readonly EstoqueApiService _estoqueApiService;
        public NotaFiscalService(AppDbContext context, IMapper mapper, BaixaEstoquePublisher baixaEstoquePublisher, EstoqueApiService estoqueApiService)
        {
            _context = context;
            _mapper = mapper;
            _baixaEstoquePublisher = baixaEstoquePublisher;
            _estoqueApiService = estoqueApiService;
        }

        public async Task<NotaFiscal> Create(NotaFiscalCreateDTO notaFiscalCreateDTO)
        {
            await _estoqueApiService.ValidarProdutosSaldo(
                notaFiscalCreateDTO.Itens.Select(item => (item.ProdutoId, item.Quantidade))
            );

            var notaFiscal = _mapper.Map<NotaFiscal>(notaFiscalCreateDTO);
            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();
            return notaFiscal;
        }

        public async Task<IEnumerable<NotaFiscal>> FindAll()
        {
            var notasFiscais = _context.NotasFiscais
                .Include(notaFiscal => notaFiscal.Itens)
                .ToList();
            return notasFiscais;
        }

        public async Task<NotaFiscal> FindOne(int id)
        {
            var notaFiscal = await _context.NotasFiscais
                .Include(notaFiscal => notaFiscal.Itens)
                .FirstOrDefaultAsync(notaFiscal => notaFiscal.Id == id);

            if (notaFiscal == null)
            {
                var message = "Nota Fiscal não encontrada.";
                throw new ErrorServiceException(c => c.NotFound(new
                {
                    error = true,
                    message,
                    code = 404
                }), message);
            }
            return notaFiscal;
        }
    
        public async Task<NotaFiscal> ImpressaoNotaFiscal(int id)
        {
            var notaFiscal = await FindOne(id);

            if(notaFiscal.Status != "Aberta" && notaFiscal.Status != "Erro")
            {
                var message = "Nota Fiscal nao pode ser impressa, pois nao esta aberta ou com erro para reprocessamento.";
                throw new ErrorServiceException(c => c.BadRequest(new
                {
                    error = true,
                    message,
                    code = 400
                }), message);
            }

            var itens = notaFiscal.Itens;

            if (!itens.Any()){
                var message = "A nota fiscal precisa ter itens.";
                throw new ErrorServiceException(c => c.BadRequest(new
                {
                    error = true,
                    message,
                    code = 400
                }), message);
            }

            var evento = new BaixaEstoqueSolicitadaEvent
            {
                EventoId = notaFiscal.Id.ToString(),
                EventoType = nameof(BaixaEstoqueSolicitadaEvent),
                DataSolicitacao = DateTime.UtcNow,
                Itens = itens.Select(item => new BaixaEstoqueItemEvent
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade
                }).ToList()
            };

            notaFiscal.Status = "Processando";
            notaFiscal.Observacao = "Baixa de estoque enviada para processamento.";
            await _context.SaveChangesAsync();

            try
            {
                await _baixaEstoquePublisher.PublishAsync(evento);
            }
            catch (Exception ex)
            {
                notaFiscal.Status = "Erro";
                notaFiscal.Observacao = $"Nao foi possivel enviar a baixa de estoque para processamento: {ex.Message}";
                await _context.SaveChangesAsync();

                var message = notaFiscal.Observacao;
                throw new ErrorServiceException(c => c.StatusCode(503, new
                {
                    error = true,
                    message,
                    code = 503
                }), message);
            }

            return notaFiscal;
        }

        public async Task UpdateStatus(int id, string status, string? observacao = null)
        {
            var notaFiscal = await FindOne(id);

            if(notaFiscal.Status == "Cancelada")
            {
                var message = "Nota Fiscal não pode ser atualizada, pois está cancelada.";
                throw new ErrorServiceException(c => c.BadRequest(new
                {
                    error = true,
                    message,
                    code = 400
                }), message);
            }

            notaFiscal.Status = status;
            notaFiscal.Observacao = observacao;
            _context.NotasFiscais.Update(notaFiscal);
            await _context.SaveChangesAsync();
        }
    }
}
