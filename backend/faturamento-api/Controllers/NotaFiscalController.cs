using estoque_api.Exceptions;
using faturamento_api.DTOs;
using faturamento_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace faturamento_api.Controllers
{
    [Route("api/[controller]")]
    public class NotaFiscalController : Controller
    {
        private readonly NotaFiscalService _notaFiscalService;
        private readonly NotaFiscalSseService _sseService;

        public NotaFiscalController(NotaFiscalService notaFiscalService, NotaFiscalSseService sseService)
        {
            _notaFiscalService = notaFiscalService;
            _sseService = sseService;
        }

        [HttpGet("{id}/stream")]
        public async Task Stream(int id, CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";

            Response.Headers.CacheControl = "no-cache";

            Response.Headers.Append(
                "X-Accel-Buffering",
                "no"
            );

            var reader = _sseService.Subscribe(id);

            try
            {
                await foreach (
                    var evento in reader.ReadAllAsync(
                        cancellationToken))
                {
                    var json =
                        JsonSerializer.Serialize(evento);

                    await Response.WriteAsync(
                        $"data: {json}\n\n",
                        cancellationToken
                    );

                    await Response.Body.FlushAsync(
                        cancellationToken
                    );
                }
            }
            catch (OperationCanceledException)
            {
                // navegador fechou a conexão
            }
            finally
            {
                _sseService.Unsubscribe(id);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> FindOne(int id)
        {
            try
            {
                var notaFiscal = await _notaFiscalService.FindOne(id);

                return Ok(new
                {
                    error = false,
                    message = "Nota fiscal encontrada com sucesso.",
                    code = 200,
                    data = notaFiscal
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindAll()
        {
            try
            {
                var notasFiscais = await _notaFiscalService.FindAll();

                return Ok(new
                {
                    error = false,
                    message = "Notas fiscais encontradas com sucesso.",
                    code = 200,
                    data = notasFiscais
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotaFiscalCreateDTO notaFiscal)
        {
            try
            {
                var result = await _notaFiscalService.Create(notaFiscal);

                return Ok(new
                {
                    error = false,
                    message = $"Nota fiscal {result.Numero} criada com sucesso.",
                    code = 201,
                    data = result
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }

        [HttpPost("{id}/Impressao")]
        public async Task<IActionResult> ImpressaoNotaFiscal(int id)
        {
            try
            {
                var result = await _notaFiscalService.ImpressaoNotaFiscal(id);

                return Ok(new
                {
                    error = false,
                    message = $"Solicitação de baixa de estoque enviada para a nota fiscal {result.Numero}.",
                    code = 202,
                    data = result
                });
            }
            catch (ErrorServiceException ex)
            {
                return ex.ToActionResult(this);
            }
        }
    }
}
