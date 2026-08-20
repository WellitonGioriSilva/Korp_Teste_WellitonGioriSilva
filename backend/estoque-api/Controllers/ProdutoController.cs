using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using estoque_api.DTOs;
using estoque_api.Exceptions;
using estoque_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace estoque_api.Controllers
{
    [Route("api/[controller]")]
    public class ProdutoController : Controller
    {
        private readonly ProdutoService _produtoService;
        public ProdutoController(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }
 
        [HttpGet("{id}")]
        public async Task<IActionResult> FindOne(int id)
        {
            try
            {
                var produto = await _produtoService.FindOne(id);

                return Ok(new
                {
                    error = false,
                    message = "Produto encontrado com sucesso.",
                    code = 200,
                    data = produto
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }    
        }

        [HttpGet]
        public async Task<IActionResult> FindAll([FromQuery] string? descricao)
        {
            try
            {
                var produtos = await _produtoService.FindAll(descricao);
                return Ok(new
                {
                    error = false,
                    message = "Produtos encontrados com sucesso.",
                    code = 200,
                    data = produtos
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProdutoCreateDTO produto)
        {
            try
            {
                var result = await _produtoService.Create(produto);
                return Ok(new {
                    error = false,
                    message = $"Produto {result.Descricao} criado com sucesso.",
                    code = 201
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProdutoUpdateDTO produto)
        {
            try
            {
                var result = await _produtoService.Update(id, produto);
                return Ok(new {
                    error = false,
                    message = $"Produto {result.Descricao} atualizado com sucesso.",
                    code = 200
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }
    }
}
