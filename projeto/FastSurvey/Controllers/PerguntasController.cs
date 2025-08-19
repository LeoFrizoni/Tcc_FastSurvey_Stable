// FASTSURVEY/Controllers/PerguntasController.cs
using FASTSURVEY.Models;
using FASTSURVEY.Models.DTO;
using FASTSURVEY.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    public class PerguntasController : ControllerBase
    {
        private readonly ServicePerguntas _svcPerguntas;

        public PerguntasController(ServicePerguntas svcPerguntas)
        {
            _svcPerguntas = svcPerguntas;
        }

        // GET: api/pesquisas/{pesquisaId}/perguntas
        [HttpGet("pesquisas/{pesquisaId:int}/perguntas")]
        public async Task<IActionResult> ListarPorPesquisa(int pesquisaId, CancellationToken ct)
        {
            var list = await _svcPerguntas.ListarPorPesquisaAsync(pesquisaId, ct);
            if (list == null || list.Count == 0) return NotFound("Nenhuma pergunta encontrada para a pesquisa.");
            return Ok(list);
        }

        // GET: api/perguntas/{id}
        [HttpGet("perguntas/{id:int}")]
        public async Task<IActionResult> BuscarPorId(int id, CancellationToken ct)
        {
            var p = await _svcPerguntas.BuscarPerguntaPorIdAsync(id, ct);
            if (p == null) return NotFound("Pergunta não encontrada.");
            return Ok(p);
        }

        // POST: api/pesquisas/{pesquisaId}/perguntas/discursiva
        [HttpPost("pesquisas/{pesquisaId:int}/perguntas/discursiva")]
        public async Task<IActionResult> CriarDiscursiva(int pesquisaId, [FromBody] PerguntaDiscursivaDto dto, CancellationToken ct)
        {
            try
            {
                var p = await _svcPerguntas.CadastrarDiscursivaAsync(pesquisaId, dto, ct);
                return CreatedAtAction(nameof(BuscarPorId), new { id = p.perguntaid }, p);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // POST: api/pesquisas/{pesquisaId}/perguntas/objetiva
        [HttpPost("pesquisas/{pesquisaId:int}/perguntas/objetiva")]
        public async Task<IActionResult> CriarObjetiva(int pesquisaId, [FromBody] PerguntaObjetivaDto dto, CancellationToken ct)
        {
            try
            {
                var p = await _svcPerguntas.CadastrarObjetivaAsync(pesquisaId, dto, ct);
                return CreatedAtAction(nameof(BuscarPorId), new { id = p.perguntaid }, p);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // POST: api/pesquisas/{pesquisaId}/perguntas/multipla
        [HttpPost("pesquisas/{pesquisaId:int}/perguntas/multipla")]
        public async Task<IActionResult> CriarMultipla(int pesquisaId, [FromBody] PerguntaMultiplaEscolhaDto dto, CancellationToken ct)
        {
            try
            {
                var p = await _svcPerguntas.CadastrarMultiplaAsync(pesquisaId, dto, ct);
                return CreatedAtAction(nameof(BuscarPorId), new { id = p.perguntaid }, p);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // PUT: api/perguntas/{id}
        // (atualiza campos simples da pergunta — texto, flags; use endpoints de gabarito/opções para o resto)
        [HttpPut("perguntas/{id:int}")]
        public async Task<IActionResult> AtualizarPergunta(int id, [FromBody] PerguntaRecriarDto dto, CancellationToken ct)
        {
            try
            {
                var atual = await _svcPerguntas.BuscarPerguntaPorIdAsync(id, ct);
                if (atual == null) return NotFound("Pergunta não encontrada.");

                if (!string.IsNullOrWhiteSpace(dto.Titulo)) atual.texto = dto.Titulo.Trim();
                if (dto.TemGabarito.HasValue) atual.temgabarito = dto.TemGabarito.Value;
                if (dto.PermitirMultiplaSelecao.HasValue) atual.permitemultiplaselecao = dto.PermitirMultiplaSelecao.Value;

                var updated = await _svcPerguntas.AtualizarPerguntaAsync(atual, ct);
                return Ok(updated);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // PUT: api/perguntas/{id}/opcoes
        // Body: array de { texto, correta } — substitui todas as opções
        [HttpPut("perguntas/{id:int}/opcoes")]
        public async Task<IActionResult> RecriarOpcoes(int id, [FromBody] List<OpcaoRecriarDto> novas, CancellationToken ct)
        {
            try
            {
                var tuples = (novas ?? new List<OpcaoRecriarDto>())
                    .Select(x => (x.Texto, x.Correta));
                await _svcPerguntas.RecriarOpcoesAsync(id, tuples, ct);
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // PUT: api/perguntas/{id}/gabarito/objetiva?corretaIndex=0
        [HttpPut("perguntas/{id:int}/gabarito/objetiva")]
        public async Task<IActionResult> DefinirGabaritoObjetiva(int id, [FromQuery] int? corretaIndex, CancellationToken ct)
        {
            try
            {
                await _svcPerguntas.AtualizarGabaritoObjetivaAsync(id, corretaIndex, ct);
                return NoContent();
            }
            catch (ArgumentOutOfRangeException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // PUT: api/perguntas/{id}/gabarito/multipla
        // Body: array de índices corretos (int[])
        [HttpPut("perguntas/{id:int}/gabarito/multipla")]
        public async Task<IActionResult> DefinirGabaritoMultipla(int id, [FromBody] int[] corretasIdx, CancellationToken ct)
        {
            try
            {
                await _svcPerguntas.AtualizarGabaritoMultiplaAsync(id, corretasIdx ?? Array.Empty<int>(), ct);
                return NoContent();
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return Problem(ex.Message); }
        }

        // DELETE: api/perguntas/{id}
        [HttpDelete("perguntas/{id:int}")]
        public async Task<IActionResult> Excluir(int id, CancellationToken ct)
        {
            try
            {
                await _svcPerguntas.ExcluirPerguntaAsync(id, ct);
                return NoContent();
            }
            catch (Exception ex) { return Problem(ex.Message); }
        }
    }

    // DTOs auxiliares do controller
    public class PerguntaRecriarDto
    {
        public string? Titulo { get; set; }
        public bool? TemGabarito { get; set; }
        public bool? PermitirMultiplaSelecao { get; set; }
    }

    public class OpcaoRecriarDto
    {
        public string Texto { get; set; } = "";
        public bool Correta { get; set; }
    }
}
