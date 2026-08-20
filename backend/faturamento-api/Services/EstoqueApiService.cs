using System.Net.Http.Json;
using estoque_api.Exceptions;
using faturamento_api.DTOs;

namespace faturamento_api.Services
{
    public class EstoqueApiService
    {
        private readonly HttpClient _httpClient;
        public EstoqueApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task ValidarProdutosSaldo(IEnumerable<(int ProdutoId, int Quantidade)> produtos){
            var produtosAgrupados = produtos
                .GroupBy(produto => produto.ProdutoId)
                .Select(grupo => (
                    ProdutoId: grupo.Key,
                    Quantidade: grupo.Sum(produto => produto.Quantidade)
                ));

            foreach (var produto in produtosAgrupados)
            {
                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.GetAsync($"api/Produto/{produto.ProdutoId}");
                }
                catch (HttpRequestException)
                {
                    const string message = "Nao foi possivel conectar ao estoque-api para validar os produtos.";
                    throw new ErrorServiceException(c => c.StatusCode(503, new
                    {
                        error = true,
                        message,
                        code = 503
                    }), message);
                }
                catch (TaskCanceledException)
                {
                    const string message = "Tempo limite ao consultar o estoque-api para validar os produtos.";
                    throw new ErrorServiceException(c => c.StatusCode(503, new
                    {
                        error = true,
                        message,
                        code = 503
                    }), message);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var message = $"Produto {produto.ProdutoId} não encontrado.";
                    throw new ErrorServiceException(c => c.BadRequest(new
                    {
                        error = true,
                        message,
                        code = 400
                    }), message);
                }

                var produtoResponse = await response.Content.ReadFromJsonAsync<ProdutoResponse>();

                if (produtoResponse is null)
                {
                    var message = $"Erro ao consultar o produto {produto.ProdutoId}.";
                    throw new ErrorServiceException(c => c.BadRequest(new
                    {
                        error = true,
                        message,
                        code = 400
                    }), message);
                }

                if (produtoResponse.Data is null)
                {
                    var message = $"Resposta invalida ao consultar o produto {produto.ProdutoId}.";
                    throw new ErrorServiceException(c => c.BadRequest(new
                    {
                        error = true,
                        message,
                        code = 400
                    }), message);
                }

                if (produtoResponse.Data.Saldo < produto.Quantidade)
                {
                    var message = $"Produto {produto.ProdutoId} sem saldo suficiente.";
                    throw new ErrorServiceException(c => c.BadRequest(new
                    {
                        error = true,
                        message,
                        code = 400
                    }), message);
                }
            }
        }
    }
}
