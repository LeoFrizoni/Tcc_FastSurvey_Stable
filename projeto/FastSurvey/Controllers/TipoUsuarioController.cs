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
    [Route("api/[controller]")] // /api/tipousuario
    [Produces("application/json")]
    public class TipoUsuarioController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly TipoUsuarioRepository _repo;

        public TipoUsuarioController(FastSurveyContext context, TipoUsuarioRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        // POST: /api/tipousuario
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Tipousuario body, CancellationToken ct)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Tipousuario1))
                return BadRequest("O nome do tipo de usuário é obrigatório.");

            var nome = body.Tipousuario1.Trim();

            var existe = await _context.Tipousuario
                .AsNoTracking()
                .AnyAsync(t => EF.Functions.ILike(t.Tipousuario1, nome), ct);
            if (existe)
                return Conflict("Já existe um tipo de usuário com este nome.");

            body.Tipousuario1 = nome;

            var criado = await _repo.AddAsync(body);

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = criado.Tipousuarioid },
                new { criado.Tipousuarioid, tipousuario = criado.Tipousuario1 }
            );
        }

        // GET: /api/tipousuario   (retorna { id, nome })
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var lista = await _context.Tipousuario
                .AsNoTracking()
                .OrderBy(t => t.Tipousuarioid)
                .Select(t => new
                {
                    id = t.Tipousuarioid,
                    nome = t.Tipousuario1
                })
                .ToListAsync(ct);

            return Ok(lista);
        }


        // GET: /api/tipousuario/{id}   (retorna { id, nome })
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPorId(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var tipo = await _context.Tipousuario
                .AsNoTracking()
                .Where(t => t.Tipousuarioid == id)
                .Select(t => new
                {
                    id = t.Tipousuarioid,
                    nome = t.Tipousuario1
                })
                .FirstOrDefaultAsync(ct);

            return tipo is null ? NotFound("Tipo de usuário não encontrado.") : Ok(tipo);
        }

        // PUT: /api/tipousuario/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tipousuario body, CancellationToken ct)
        {
            if (id <= 0 || body == null)
                return BadRequest("Dados inválidos para alteração.");

            // se o body trouxer a PK, garanta que bate com a rota
            if (body.Tipousuarioid != 0 && body.Tipousuarioid != id)
                return BadRequest("ID do corpo não confere com a rota.");

            var nome = body.Tipousuario1?.Trim();
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest("O nome do tipo de usuário é obrigatório.");

            var existente = await _context.Tipousuario.FirstOrDefaultAsync(t => t.Tipousuarioid == id, ct);
            if (existente == null)
                return NotFound("Tipo de usuário não encontrado.");

            var existeOutro = await _context.Tipousuario
                .AsNoTracking()
                .AnyAsync(t => t.Tipousuarioid != id && EF.Functions.ILike(t.Tipousuario1, nome), ct);
            if (existeOutro)
                return Conflict("Já existe outro tipo de usuário com este nome.");

            existente.Tipousuario1 = nome;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE: /api/tipousuario/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var existente = await _context.Tipousuario.FirstOrDefaultAsync(t => t.Tipousuarioid == id, ct);
            if (existente == null)
                return NotFound("Tipo de usuário não encontrado.");

            _context.Tipousuario.Remove(existente);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}