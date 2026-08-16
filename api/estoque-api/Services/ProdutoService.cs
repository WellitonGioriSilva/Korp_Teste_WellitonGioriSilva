using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using estoque_api.DataContext;
using estoque_api.DTOs;
using estoque_api.Exceptions;
using estoque_api.Models;

namespace estoque_api.Services
{
    public class ProdutoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ProdutoService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<Produto> FindOne(int id)
        {
            try
            {
                var produto = _context.Produtos.Find(id);
                if (produto == null)
                {
                    throw new ErrorServiceException(c => c.NotFound(new
                    {
                        error = true,
                        message = "Produto não encontrado.",
                        code = 404
                    }));
                }
                return Task.FromResult<Produto>(produto);
            }
            catch
            {
                throw;
            }
        }

        public Task<IEnumerable<Produto>> FindAll()
        {
            try
            {
                var produtos = _context.Produtos.ToList();
                return Task.FromResult<IEnumerable<Produto>>(produtos);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Produto> Create(ProdutoCreateDTO produtoDto)
        {
            try
            {
                var newProduto = _mapper.Map<Produto>(produtoDto);

                _context.Produtos.Add(newProduto);
                await _context.SaveChangesAsync();

                return newProduto;
            }catch
            {
                throw;
            }
        }

        public async Task<Produto> Update(int id, ProdutoUpdateDTO produtoDto)
        {
            try
            {
                var produto = FindOne(id).Result;

                _mapper.Map(produtoDto, produto);
                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync();

                return produto;
            }
            catch
            {
                throw;
            }
        }

        public Task<bool> Delete(int id)
        {
            return Task.FromResult<bool>(true);
        }
    }
}