using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TipoPerguntaController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryTipoPergunta _repo;

        public TipoPerguntaController(FastSurveyContext context)
        {
            _context = context;
            _repo = new RepositoryTipoPergunta(context, true); // true => AsNoTracking padrão no repo
        }

        // POST: api/TipoPergunta/AdicionarTipoPergunta
        [HttpPost("AdicionarTipoPergunta")]
        public async Task<IActionResult> Post([FromBody] tipopergunta body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.tipopergunta1))
                return BadRequest("Descrição do tipo de pergunta é obrigatória.");

            var descricao = body.tipopergunta1.Trim();

            // valida duplicidade (case-insensitive)
            var jaExiste = await _context.tipopergunta
                .AsNoTracking()
                .AnyAsync(t => EF.Functions.ILike(t.tipopergunta1, descricao));
            if (jaExiste)
                return Conflict("Já existe um tipo de pergunta com esta descrição.");

            try
            {
                body.tipopergunta1 = descricao;
                var criado = await _repo.IncluirAsync(body);

                return CreatedAtAction(nameof(GetPorId),
                    new { id = criado.tipoperguntaid },
                    new { criado.tipoperguntaid, tipopergunta = criado.tipopergunta1, criado.desabilitado });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao inserir tipo de pergunta: {ex.Message}");
            }
        }

        // GET: api/TipoPergunta/ListarTipoPergunta
        [HttpGet("ListarTipoPergunta")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var todos = await _context.tipopergunta
                    .AsNoTracking()
                    .OrderBy(t => t.tipoperguntaid)
                    .Select(t => new
                    {
                        t.tipoperguntaid,
                        tipopergunta = t.tipopergunta1,
                        t.desabilitado
                    })
                    .ToListAsync();

                return Ok(todos);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao listar tipos de pergunta: {ex.Message}");
            }
        }

        // GET: api/TipoPergunta/SelecionarTipoPerguntaPorId/5
        [HttpGet("SelecionarTipoPerguntaPorId/{id:int}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var tp = await _context.tipopergunta
                    .AsNoTracking()
                    .Where(x => x.tipoperguntaid == id)
                    .Select(x => new
                    {
                        x.tipoperguntaid,
                        tipopergunta = x.tipopergunta1,
                        x.desabilitado
                    })
                    .FirstOrDefaultAsync();

                return tp is null
                    ? NotFound("Tipo de pergunta não encontrado.")
                    : Ok(tp);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao selecionar tipo de pergunta: {ex.Message}");
            }
        }

        // PUT: api/TipoPergunta/AlterarTipoPerguntaPorId/5
        [HttpPut("AlterarTipoPerguntaPorId/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] tipopergunta body)
        {
            if (id <= 0 || body == null || id != body.tipoperguntaid)
                return BadRequest("Dados inválidos.");

            var novaDescricao = body.tipopergunta1?.Trim();
            if (string.IsNullOrWhiteSpace(novaDescricao))
                return BadRequest("Descrição do tipo de pergunta é obrigatória.");

            // valida duplicidade (case-insensitive) excluindo o próprio ID
            var jaExiste = await _context.tipopergunta
                .AsNoTracking()
                .AnyAsync(t => t.tipoperguntaid != id && EF.Functions.ILike(t.tipopergunta1, novaDescricao));
            if (jaExiste)
                return Conflict("Já existe outro tipo de pergunta com esta descrição.");

            try
            {
                var existente = await _context.tipopergunta.FirstOrDefaultAsync(t => t.tipoperguntaid == id);
                if (existente == null) return NotFound("Tipo de pergunta não encontrado.");

                existente.tipopergunta1 = novaDescricao;
                existente.desabilitado = body.desabilitado;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de pergunta: {ex.Message}");
            }
        }

        // DELETE: api/TipoPergunta/ExcluirTipoPergunta/5
        [HttpDelete("ExcluirTipoPergunta/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var existente = await _context.tipopergunta.FirstOrDefaultAsync(t => t.tipoperguntaid == id);
                if (existente == null) return NotFound("Tipo de pergunta não encontrado.");

                _context.tipopergunta.Remove(existente);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir tipo de pergunta: {ex.Message}");
            }
        }
    }
}
