using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PastasController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        public PastasController(FastSurveyContext context) => _context = context;

        // DTOs
        public record PastaCreateDto(string nome, int loginid);
        public record PastaUpdateDto(string nome);
        public record PastaDto(int pastaId, string nome);

        /// <summary>
        /// Lista pastas (opcionalmente filtradas por loginid). 
        /// Use ?includeCounts=true para incluir total de pesquisas por pasta.
        /// </summary>
        // GET: api/pastas?loginid=16&includeCounts=true
        [HttpGet]
        public async Task<IActionResult> GetPastas([FromQuery] int? loginid, [FromQuery] bool includeCounts = false)
        {
            var q = _context.pastas.AsNoTracking().AsQueryable();
            if (loginid.HasValue) q = q.Where(p => p.loginid == loginid.Value);

            if (includeCounts)
            {
                var list = await q
                    .OrderBy(p => p.nome)
                    .Select(p => new
                    {
                        pastaId = p.pastaid,
                        nome = p.nome,
                        totalPesquisas = _context.pesquisas.Count(ps => ps.pastaid == p.pastaid)
                    })
                    .ToListAsync();

                return Ok(list);
            }
            else
            {
                var list = await q
                    .OrderBy(p => p.nome)
                    .Select(p => new PastaDto(p.pastaid, p.nome))
                    .ToListAsync();

                return Ok(list);
            }
        }

        /// <summary>
        /// Detalhe da pasta. Use ?includeCounts=true para incluir total de pesquisas.
        /// </summary>
        // GET: api/pastas/5?includeCounts=true
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPastaById(int id, [FromQuery] bool includeCounts = false)
        {
            var p = await _context.pastas.AsNoTracking()
                .Where(x => x.pastaid == id)
                .Select(x => new
                {
                    pastaId = x.pastaid,
                    nome = x.nome,
                    totalPesquisas = includeCounts
                        ? _context.pesquisas.Count(ps => ps.pastaid == x.pastaid)
                        : (int?)null
                })
                .FirstOrDefaultAsync();

            return p is null ? NotFound("Pasta não encontrada.") : Ok(p);
        }

        /// <summary>Cria uma pasta para o usuário (nome único por usuário, case-insensitive).</summary>
        // POST: api/pastas
        [HttpPost]
        public async Task<IActionResult> PostPasta([FromBody] PastaCreateDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.nome))
                return BadRequest("Nome é obrigatório.");
            if (dto.nome.Trim().Length > 100)
                return BadRequest("Nome não pode exceder 100 caracteres.");
            if (dto.loginid <= 0)
                return BadRequest("loginid inválido.");

            // valida usuário
            bool existeLogin = await _context.login.AnyAsync(l => l.loginid == dto.loginid);
            if (!existeLogin) return NotFound("Usuário (loginid) não encontrado.");

            var nomeTrim = dto.nome.Trim();

            // duplicidade por usuário (case-insensitive; PostgreSQL)
            var duplicada = await _context.pastas
                .AsNoTracking()
                .AnyAsync(p => p.loginid == dto.loginid && EF.Functions.ILike(p.nome, nomeTrim));
            if (duplicada)
                return Conflict("Já existe uma pasta com esse nome para este usuário.");

            var pasta = new pastas { nome = nomeTrim, loginid = dto.loginid };
            _context.pastas.Add(pasta);
            await _context.SaveChangesAsync();

            var result = new PastaDto(pasta.pastaid, pasta.nome);
            return CreatedAtAction(nameof(GetPastaById), new { id = pasta.pastaid }, result);
        }

        /// <summary>Renomeia a pasta (mantém unicidade por usuário).</summary>
        // PATCH: api/pastas/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> RenomearPasta(int id, [FromBody] PastaUpdateDto dto)
        {
            if (id <= 0) return BadRequest("ID inválido.");
            if (dto is null || string.IsNullOrWhiteSpace(dto.nome))
                return BadRequest("Nome é obrigatório.");
            var novoNome = dto.nome.Trim();
            if (novoNome.Length > 100)
                return BadRequest("Nome não pode exceder 100 caracteres.");

            var pasta = await _context.pastas.FirstOrDefaultAsync(p => p.pastaid == id);
            if (pasta is null) return NotFound("Pasta não encontrada.");

            // duplicidade por usuário
            var duplicada = await _context.pastas
                .AsNoTracking()
                .AnyAsync(p => p.loginid == pasta.loginid &&
                               p.pastaid != pasta.pastaid &&
                               EF.Functions.ILike(p.nome, novoNome));
            if (duplicada)
                return Conflict("Já existe uma pasta com esse nome para este usuário.");

            pasta.nome = novoNome;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Exclui a pasta. As pesquisas vinculadas terão pastaId definido como NULL
        /// (conforme FK com ON DELETE SET NULL).
        /// </summary>
        // DELETE: api/pastas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePasta(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var pasta = await _context.pastas.FirstOrDefaultAsync(p => p.pastaid == id);
            if (pasta is null) return NotFound("Pasta não encontrada.");

            _context.pastas.Remove(pasta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
