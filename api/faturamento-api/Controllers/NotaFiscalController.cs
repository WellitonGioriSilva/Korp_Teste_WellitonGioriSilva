using estoque_api.Exceptions;
using faturamento_api.DTOs;
using faturamento_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace faturamento_api.Controllers
{
    [Route("[controller]")]
    public class NotaFiscalController : Controller
    {
        private readonly NotaFiscalService _notaFiscalService;

        public NotaFiscalController(NotaFiscalService notaFiscalService)
        {
            _notaFiscalService = notaFiscalService;
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

        [HttpPost("{id}/impressao")]
        public async Task<IActionResult> ImpressaoNotaFiscal(int id)
        {
            try
            {
                var result = await _notaFiscalService.ImpressaoNotaFiscal(id);

                return Ok(new
                {
                    error = false,
                    message = $"Solicitação de baixa de estoque enviada para a nota fiscal {result.Numero}.",
                    code = 200,
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
