// FASTSURVEY/Controllers/RespostasController.cs
using FASTSURVEY.Models.DTO;
using FASTSURVEY.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RespostasController : ControllerBase
    {
        private readonly ServiceRespostas _svcRespostas;

        public RespostasController(ServiceRespostas svcRespostas)
        {
            _svcRespostas = svcRespostas;
        }

        // GET: api/Respostas
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _svcRespostas.ListarTodasRespostasAsync(ct);
            if (list == null || list.Count == 0) return NotFound("Nenhuma resposta encontrada.");
            return Ok(list);
        }

        // GET: api/Respostas/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var r = await _svcRespostas.ObterRespostaPorIdAsync(id, ct);
            if (r == null) return NotFound("Resposta não encontrada.");
            return Ok(r);
        }

        // POST: api/Respostas/lote
        // Body: RespostasLoteVM
        [HttpPost("lote")]
        public async Task<IActionResult> GravarLote([FromBody] RespostasLoteVM vm, CancellationToken ct)
        {
            try
            {
                var resultado = await _svcRespostas.GravarLoteAsync(vm, ct);
                return Ok(resultado);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem($"Erro ao gravar respostas: {ex.Message}"); }
        }

        // DELETE: api/Respostas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _svcRespostas.ExcluirRespostaAsync(id, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao excluir: {ex.Message}");
            }
        }
    }
}
