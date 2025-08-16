using FASTSURVEY.Models;
using FASTSURVEY.Services;
using Microsoft.AspNetCore.Http;
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
    public class PesquisasController : ControllerBase
    {
        private FastSurveyContext _context;
        private ServicePesquisas _servicePesquisa;

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
                {
                    return NotFound("Nenhuma pesquisa encontrada.");
                }

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
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var pesquisa = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisa == null)
                {
                    return NotFound("Pesquisa não encontrada.");
                }

                return Ok(pesquisa);
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
                    .Where(p => p.loginid == loginId)
                    .Select(p => new
                    {
                        pesquisaid = p.pesquisaid,
                        titulo = p.titulo,
                        descricao = p.descricao,
                        tipoPesquisa = new
                        {
                            descricao = p.tipopesquisa != null ? p.tipopesquisa.tipopesquisa1 : ""
                        },
                        perguntas = p.perguntas.Select(pergunta => new
                        {
                            perguntaid = pergunta.perguntaid,
                            titulo = pergunta.texto,
                            tipo = pergunta.tipoperguntaid == 1 ? "discursiva" : "multipla",
                            opcoes = pergunta.opcoespergunta.Select(o => new
                            {
                                opcaoid = o.opcaoid,
                                texto = o.texto
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
            {
                return BadRequest("Dados da pesquisa são inválidos.");
            }

            try
            {
                var novaPesquisa = await _servicePesquisa.CadastrarPesquisaAsync(pesquisaVM);
                // Retorne apenas os campos primitivos, nunca o objeto do EF
                return Ok(new {
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
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var pesquisaExistente = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisaExistente == null)
                {
                    return NotFound("Pesquisa não encontrada.");
                }

                var pesquisaAtualizada = await _servicePesquisa.AtualizarPesquisaAsync(pesquisaVM);
                // Retorne apenas campos primitivos
                return Ok(new {
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

        // GET: api/Pesquisas/{id}

        // DELETE: api/Pesquisas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var pesquisa = await _servicePesquisa.BuscarPesquisaPorIdAsync(id);
                if (pesquisa == null)
                {
                    return NotFound("Pesquisa não encontrada.");
                }

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
