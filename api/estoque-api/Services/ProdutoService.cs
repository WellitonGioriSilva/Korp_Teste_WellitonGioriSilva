using AutoMapper;
using Contracts.Events;
using estoque_api.DataContext;
using estoque_api.DTOs;
using estoque_api.Exceptions;
using estoque_api.Models;
using Microsoft.EntityFrameworkCore;

namespace estoque_api.Services
{
    public class ProdutoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly EventoProcessadoService _eventoProcessadoService;

        public ProdutoService(AppDbContext context, IMapper mapper, EventoProcessadoService eventoProcessadoService)
        {
            _context = context;
            _mapper = mapper;
            _eventoProcessadoService = eventoProcessadoService;
        }

        public async Task<Produto> FindOne(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                const string message = "Produto não encontrado.";
                throw new ErrorServiceException(c => c.NotFound(new
                {
                    error = true,
                    message,
                    code = 404
                }), message);
            }

            return produto;
        }

        public Task<IEnumerable<Produto>> FindAll()
        {
            var produtos = _context.Produtos.ToList();
            return Task.FromResult<IEnumerable<Produto>>(produtos);
        }

        public async Task<Produto> Create(ProdutoCreateDTO produtoDto)
        {
            var newProduto = _mapper.Map<Produto>(produtoDto);

            _context.Produtos.Add(newProduto);
            await _context.SaveChangesAsync();

            return newProduto;
        }

        public async Task<Produto> Update(int id, ProdutoUpdateDTO produtoDto)
        {
            var produto = await FindOne(id);

            _mapper.Map(produtoDto, produto);
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();

            return produto;
        }

        public Task<bool> Delete(int id)
        {
            return Task.FromResult(true);
        }

        // public Task<bool> BaixarEstoque(int produtoId, int quantidade)
        // {
        //     return BaixarEstoque(new List<BaixaEstoqueItemEvent>
        //     {
        //         new BaixaEstoqueItemEvent
        //         {
        //             ProdutoId = produtoId,
        //             Quantidade = quantidade
        //         }
        //     });
        // }

        public async Task<bool> BaixarEstoque(BaixaEstoqueSolicitadaEvent evento)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var eventoProcessado = new EventoProcessado(){EventoId = evento.EventoId, EventoType = evento.EventoType};
                if (await _eventoProcessadoService.EventoProcessado(eventoProcessado))
                {
                    await transaction.CommitAsync();
                    return true;
                }

                var itensBaixa = evento.Itens.ToList();

                if (!itensBaixa.Any())
                {
                    const string message = "Nenhum item informado para baixa de estoque.";
                    throw new ErrorServiceException(c => c.BadRequest(new
                    {
                        error = true,
                        message,
                        code = 400
                    }), message);
                }

                foreach (var item in itensBaixa)
                {
                    if (item.Quantidade <= 0)
                    {
                        var message = $"Quantidade inválida para o produto {item.ProdutoId}.";
                        throw new ErrorServiceException(c => c.BadRequest(new
                        {
                            error = true,
                            message,
                            code = 400
                        }), message);
                    }

                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                    if (produto == null)
                    {
                        var message = $"Produto {item.ProdutoId} não encontrado.";
                        throw new ErrorServiceException(c => c.NotFound(new
                        {
                            error = true,
                            message,
                            code = 404
                        }), message);
                    }

                    if (produto.Saldo < item.Quantidade)
                    {
                        var message = $"Produto {item.ProdutoId} sem saldo suficiente.";
                        throw new ErrorServiceException(c => c.BadRequest(new
                        {
                            error = true,
                            message,
                            code = 400
                        }), message);
                    }

                    produto.Saldo -= item.Quantidade;
                    _context.Produtos.Update(produto);
                }

                await _eventoProcessadoService.Create(eventoProcessado);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
