using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // -> /api/tipopesquisa
    [Produces("application/json")]
    public class TipoPesquisaController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryTipoPesquisa _repo;

        // injete via DI (Program.cs: builder.Services.AddScoped<RepositoryTipoPesquisa>();)
        public TipoPesquisaController(FastSurveyContext context, RepositoryTipoPesquisa repo)
        {
            _context = context;
            _repo = repo;
        }

        // POST: /api/tipopesquisa/AdicionarTipoPesquisa
        [HttpPost("AdicionarTipoPesquisa")]
        public async Task<IActionResult> Post([FromBody] tipopesquisa body, CancellationToken ct)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.tipopesquisa1))
                return BadRequest("Descrição do tipo de pesquisa é obrigatória.");

            var descricao = body.tipopesquisa1.Trim();

            // Duplicidade (case-insensitive)
            var existe = await _context.tipopesquisa
                .AsNoTracking()
                .AnyAsync(t => EF.Functions.ILike(t.tipopesquisa1, descricao), ct);

            if (existe)
                return Conflict("Já existe um tipo de pesquisa com esta descrição.");

            try
            {
                body.tipopesquisa1 = descricao;
                var criado = await _repo.IncluirAsync(body);

                return CreatedAtAction(nameof(GetPorId),
                    new { id = criado.tipopesquisaid },
                    new { criado.tipopesquisaid, tipopesquisa = criado.tipopesquisa1, criado.desabilitado });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao inserir tipo de pesquisa: {ex.Message}");
            }
        }

        // GET: /api/tipopesquisa/ListarTipoPesquisa
        [HttpGet("ListarTipoPesquisa")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            try
            {
                var lista = await _context.tipopesquisa
                    .AsNoTracking()
                    .OrderBy(t => t.tipopesquisaid)
                    .Select(t => new
                    {
                        t.tipopesquisaid,
                        tipopesquisa = t.tipopesquisa1,
                        t.desabilitado
                    })
                    .ToListAsync(ct);

                return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao listar tipos de pesquisa: {ex.Message}");
            }
        }

        // (Opcional) Alias mais genérico para front que espera { id, nome }
        // GET: /api/tipopesquisa/Listar
        [HttpGet("Listar")]
        public async Task<IActionResult> Listar(CancellationToken ct)
        {
            var lista = await _context.tipopesquisa
                .AsNoTracking()
                .OrderBy(t => t.tipopesquisaid)
                .Select(t => new { id = t.tipopesquisaid, nome = t.tipopesquisa1 })
                .ToListAsync(ct);

            return Ok(lista);
        }

        // GET: /api/tipopesquisa/SelecionarTipoPesquisaPorId/5
        [HttpGet("SelecionarTipoPesquisaPorId/{id:int}")]
        public async Task<IActionResult> GetPorId(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var tp = await _context.tipopesquisa
                    .AsNoTracking()
                    .Where(x => x.tipopesquisaid == id)
                    .Select(x => new
                    {
                        x.tipopesquisaid,
                        tipopesquisa = x.tipopesquisa1,
                        x.desabilitado
                    })
                    .FirstOrDefaultAsync(ct);

                return tp is null ? NotFound("Tipo de pesquisa não encontrado.") : Ok(tp);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao selecionar tipo de pesquisa: {ex.Message}");
            }
        }

        // PUT: /api/tipopesquisa/AlterarTipoPesquisaPorId/5
        [HttpPut("AlterarTipoPesquisaPorId/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] tipopesquisa body, CancellationToken ct)
        {
            if (id <= 0 || body == null || id != body.tipopesquisaid)
                return BadRequest("Dados inválidos.");

            var novaDescricao = body.tipopesquisa1?.Trim();
            if (string.IsNullOrWhiteSpace(novaDescricao))
                return BadRequest("Descrição do tipo de pesquisa é obrigatória.");

            // Duplicidade, excluindo o próprio registro
            var existeOutro = await _context.tipopesquisa
                .AsNoTracking()
                .AnyAsync(t => t.tipopesquisaid != id && EF.Functions.ILike(t.tipopesquisa1, novaDescricao), ct);

            if (existeOutro)
                return Conflict("Já existe outro tipo de pesquisa com esta descrição.");

            try
            {
                var existente = await _context.tipopesquisa.FirstOrDefaultAsync(t => t.tipopesquisaid == id, ct);
                if (existente == null) return NotFound("Tipo de pesquisa não encontrado.");

                existente.tipopesquisa1 = novaDescricao;
                existente.desabilitado = body.desabilitado;

                await _context.SaveChangesAsync(ct);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de pesquisa: {ex.Message}");
            }
        }

        // DELETE: /api/tipopesquisa/ExcluirTipoPesquisa/5
        [HttpDelete("ExcluirTipoPesquisa/{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var existente = await _context.tipopesquisa.FirstOrDefaultAsync(t => t.tipopesquisaid == id, ct);
                if (existente == null) return NotFound("Tipo de pesquisa não encontrado.");

                _context.tipopesquisa.Remove(existente);
                await _context.SaveChangesAsync(ct);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir tipo de pesquisa: {ex.Message}");
            }
        }
    }
}
