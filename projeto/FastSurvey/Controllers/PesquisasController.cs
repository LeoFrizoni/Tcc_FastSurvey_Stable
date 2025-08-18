using FASTSURVEY.Models;
using FASTSURVEY.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PesquisasController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly ServicePesquisas _servicePesquisa;

        public PesquisasController(FastSurveyContext context)
        {
            _context = context;
            _servicePesquisa = new ServicePesquisas(context);
        }

        // GET: api/Pesquisas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var pesquisas = await _servicePesquisa.ListarTodasPesquisasAsync();
                if (pesquisas == null || pesquisas.Count == 0)
                    return NotFound("Nenhuma pesquisa encontrada.");

                return Ok(pesquisas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar pesquisas: {ex.Message}");
            }
        }

        // GET: api/Pesquisas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var pesquisa = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisa == null) return NotFound("Pesquisa não encontrada.");
                return Ok(pesquisa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pesquisa: {ex.Message}");
            }
        }

        // ✅ usado pelo front (createPesquisa.jsx) para mapear perguntas por ordem
        // GET: api/Pesquisas/BuscarPesquisaPorId/{id}
        [HttpGet("BuscarPesquisaPorId/{id}")]
        public async Task<IActionResult> BuscarPesquisaPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var pesquisa = await _context.pesquisas
                    .AsNoTracking()
                    .Include(p => p.perguntas)
                        .ThenInclude(pg => pg.opcoespergunta)
                    .FirstOrDefaultAsync(p => p.pesquisaid == id);

                if (pesquisa == null) return NotFound("Pesquisa não encontrada.");

                var perguntas = pesquisa.perguntas
                    .OrderBy(pg => pg.perguntaid) // ajuste se tiver coluna de ordem
                    .Select(pg => new
                    {
                        perguntaid = pg.perguntaid,
                        tipoperguntaid = pg.tipoperguntaid,
                        texto = pg.texto,
                        temgabarito = pg.temgabarito,
                        permitemultiplaselecao = pg.permitemultiplaselecao,
                        opcoes = pg.opcoespergunta
                            .OrderBy(o => o.opcaoid)
                            .Select(o => new
                            {
                                opcaoid = o.opcaoid,
                                texto = o.texto,
                                correta = o.correta
                            }).ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    pesquisaid = pesquisa.pesquisaid,
                    titulo = pesquisa.titulo,
                    descricao = pesquisa.descricao,
                    perguntas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pesquisa: {ex.Message}");
            }
        }

        // GET: api/Pesquisas/usuario/{loginId}
        [HttpGet("usuario/{loginId}")]
        public IActionResult GetPesquisasPorUsuario(int loginId)
        {
            try
            {
                var pesquisas = _context.pesquisas
                    .AsNoTracking()
                    .Where(p => p.loginid == loginId)
                    .Select(p => new
                    {
                        pesquisaid = p.pesquisaid,
                        titulo = p.titulo,
                        descricao = p.descricao,
                        tipoPesquisa = new
                        {
                            // no seu mapeamento a propriedade é tipopesquisa1
                            descricao = p.tipopesquisa != null ? p.tipopesquisa.tipopesquisa1 : ""
                        },
                        perguntas = p.perguntas.Select(pergunta => new
                        {
                            perguntaid = pergunta.perguntaid,
                            titulo = pergunta.texto,
                            // 1: discursiva, 2: objetiva, 3: multipla
                            tipo = pergunta.tipoperguntaid == 1
                                ? "discursiva"
                                : (pergunta.tipoperguntaid == 2 ? "objetiva" : "multipla"),
                            opcoes = pergunta.opcoespergunta
                                .OrderBy(o => o.opcaoid)
                                .Select(o => new
                                {
                                    opcaoid = o.opcaoid,
                                    texto = o.texto,
                                    correta = o.correta
                                }).ToList()
                        }).ToList()
                    })
                    .ToList();

                return Ok(pesquisas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pesquisas do usuário: {ex.Message}");
            }
        }

        // POST: api/Pesquisas
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PesquisaVM pesquisaVM)
        {
            if (pesquisaVM == null || string.IsNullOrWhiteSpace(pesquisaVM.Titulo))
                return BadRequest("Dados da pesquisa são inválidos.");

            try
            {
                var novaPesquisa = await _servicePesquisa.CadastrarPesquisaAsync(pesquisaVM);
                return Ok(new
                {
                    pesquisaid = novaPesquisa.pesquisaid,
                    titulo = novaPesquisa.titulo,
                    descricao = novaPesquisa.descricao,
                    tipopesquisaid = novaPesquisa.tipopesquisaid,
                    loginid = novaPesquisa.loginid,
                    templateJson = novaPesquisa.TemplateJson
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar pesquisa: {ex.Message}");
            }
        }

        // PUT: api/Pesquisas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PesquisaVM pesquisaVM)
        {
            if (id <= 0 || pesquisaVM == null || id != pesquisaVM.CodigoPesquisa)
                return BadRequest("Dados inválidos.");

            try
            {
                var pesquisaExistente = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisaExistente == null)
                    return NotFound("Pesquisa não encontrada.");

                var pesquisaAtualizada = await _servicePesquisa.AtualizarPesquisaAsync(pesquisaVM);
                return Ok(new
                {
                    pesquisaid = pesquisaAtualizada.pesquisaid,
                    titulo = pesquisaAtualizada.titulo,
                    descricao = pesquisaAtualizada.descricao,
                    tipopesquisaid = pesquisaAtualizada.tipopesquisaid,
                    loginid = pesquisaAtualizada.loginid,
                    templateJson = pesquisaAtualizada.TemplateJson
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar pesquisa: {ex.Message}");
            }
        }

        // DELETE: api/Pesquisas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var pesquisa = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisa == null)
                    return NotFound("Pesquisa não encontrada.");

                await _servicePesquisa.ExcluirPesquisaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir pesquisa: {ex.Message}");
            }
        }
    }
}
