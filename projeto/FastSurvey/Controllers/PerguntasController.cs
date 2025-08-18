using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using SISTEMA_FASTSURVEY.MODEL.Models;        // entidades (perguntas, opcoespergunta)
using SISTEMA_FASTSURVEY.MODEL.Repositories;  // repositórios EF (RepositoryPerguntas, RepositoryOpcoesPergunta)

namespace FASTSURVEY.Controllers
{
    // DTO inline (pode mover para FASTSURVEY.Models/DTOs/DefinirGabaritoDto.cs se preferir)
    public class DefinirGabaritoDto
    {
        public bool TemGabarito { get; set; }
        public bool PermiteMultiplaSelecao { get; set; }
        public int[] OpcoesCorretas { get; set; } = Array.Empty<int>(); // lista de opcaoid
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PerguntasController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryPerguntas _repoPerguntas;
        private readonly RepositoryOpcoesPergunta _repoOpcoes;

        public PerguntasController(FastSurveyContext context)
        {
            _context = context;
            _repoPerguntas = new RepositoryPerguntas(_context, true);
            _repoOpcoes = new RepositoryOpcoesPergunta(_context, true);
        }

        // --------------------------------------------------------------------
        // CRUD BÁSICO
        // --------------------------------------------------------------------

        /// <summary>Cadastra uma pergunta.</summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] perguntas body)
        {
            if (body == null ||
                body.pesquisaid <= 0 ||
                body.tipoperguntaid <= 0 ||
                string.IsNullOrWhiteSpace(body.texto))
            {
                return BadRequest("Dados inválidos para pergunta.");
            }

            try
            {
                // Defaults defensivos (banco já tem default, mas garantimos)
                body.temgabarito = body.temgabarito;
                body.permitemultiplaselecao = body.permitemultiplaselecao;

                var criado = await _repoPerguntas.IncluirAsync(body);
                return CreatedAtAction(nameof(GetPorId), new { id = criado.perguntaid }, criado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar pergunta: {ex.Message}");
            }
        }

        /// <summary>Lista perguntas de uma pesquisa.</summary>
        [HttpGet("Pesquisa/{pesquisaId}")]
        public async Task<IActionResult> GetPorPesquisa(int pesquisaId)
        {
            if (pesquisaId <= 0) return BadRequest("ID de pesquisa inválido.");

            try
            {
                var lista = (await _repoPerguntas.SelecionarTodosAsync())
                    .Where(p => p.pesquisaid == pesquisaId)
                    .ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar perguntas: {ex.Message}");
            }
        }

        /// <summary>Obtém uma pergunta pelo ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var pergunta = await _repoPerguntas.SelecionarChaveAsync(id);
                if (pergunta == null) return NotFound("Pergunta não encontrada.");

                return Ok(pergunta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pergunta: {ex.Message}");
            }
        }

        /// <summary>Atualiza campos da pergunta (texto, tipo, flags).</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] perguntas body)
        {
            if (id <= 0 || body == null || id != body.perguntaid)
                return BadRequest("Dados inválidos para atualização.");

            try
            {
                var existente = await _repoPerguntas.SelecionarChaveAsync(id);
                if (existente == null) return NotFound("Pergunta não encontrada.");

                // Atualiza campos principais
                if (body.tipoperguntaid > 0) existente.tipoperguntaid = body.tipoperguntaid;
                if (!string.IsNullOrWhiteSpace(body.texto)) existente.texto = body.texto;

                // Flags de gabarito
                existente.temgabarito = body.temgabarito;
                existente.permitemultiplaselecao = body.permitemultiplaselecao;

                await _repoPerguntas.AlterarAsync(existente);

                // Observação: definição das opções corretas é feita no endpoint específico (gabarito)
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar pergunta: {ex.Message}");
            }
        }

        /// <summary>Exclui uma pergunta.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var pergunta = await _repoPerguntas.SelecionarChaveAsync(id);
                if (pergunta == null) return NotFound("Pergunta não encontrada.");

                await _repoPerguntas.ExcluirAsync(pergunta);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir pergunta: {ex.Message}");
            }
        }

        // --------------------------------------------------------------------
        // ENDPOINTS COMPLEMENTARES (detalhe, gabarito)
        // --------------------------------------------------------------------

        /// <summary>
        /// Retorna a pergunta com as opções (inclui flag 'correta' de cada opção).
        /// Útil para Preview/Responder.
        /// </summary>
        [HttpGet("{id}/detalhe")]
        public async Task<IActionResult> GetDetalhe(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var p = await _repoPerguntas.SelecionarChaveAsync(id);
                if (p == null) return NotFound("Pergunta não encontrada.");

                var opcoes = (await _repoOpcoes.SelecionarTodosAsync())
                    .Where(o => o.perguntaid == id)
                    .Select(o => new
                    {
                        o.opcaoid,
                        o.texto,
                        o.correta
                    })
                    .ToList();

                return Ok(new
                {
                    p.perguntaid,
                    p.pesquisaid,
                    p.tipoperguntaid,
                    p.texto,
                    p.temgabarito,
                    p.permitemultiplaselecao,
                    opcoes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao montar detalhe: {ex.Message}");
            }
        }

        /// <summary>
        /// Define/atualiza o gabarito de uma pergunta (em lote) e mantém consistência:
        /// - Se TemGabarito = false → zera todas as corretas.
        /// - Se !PermiteMultiplaSelecao → exige exatamente 1 correta.
        /// - Se PermiteMultiplaSelecao → permite 0..N corretas.
        /// </summary>
        [HttpPut("{perguntaId}/gabarito")]
        public async Task<IActionResult> DefinirGabarito(int perguntaId, [FromBody] DefinirGabaritoDto dto)
        {
            if (perguntaId <= 0) return BadRequest("ID de pergunta inválido.");
            if (dto == null) return BadRequest("Payload inválido.");

            try
            {
                var pergunta = await _repoPerguntas.SelecionarChaveAsync(perguntaId);
                if (pergunta == null) return NotFound("Pergunta não encontrada.");

                // Atualiza flags da pergunta
                pergunta.temgabarito = dto.TemGabarito;
                pergunta.permitemultiplaselecao = dto.PermiteMultiplaSelecao;
                await _repoPerguntas.AlterarAsync(pergunta);

                // Carrega opções da pergunta
                var opcoes = (await _repoOpcoes.SelecionarTodosAsync())
                    .Where(o => o.perguntaid == perguntaId)
                    .ToList();

                if (!dto.TemGabarito)
                {
                    // Não há gabarito → zera todas as corretas
                    foreach (var o in opcoes) o.correta = false;
                    foreach (var o in opcoes) await _repoOpcoes.AlterarAsync(o);
                    return NoContent();
                }

                // Com gabarito → validação
                var setCorretas = dto.OpcoesCorretas?.ToHashSet() ?? new HashSet<int>();

                if (!dto.PermiteMultiplaSelecao)
                {
                    if (setCorretas.Count != 1)
                        return BadRequest("Para esta pergunta, deve haver exatamente 1 opção correta.");
                }

                // Aplica flags
                foreach (var o in opcoes)
                {
                    o.correta = setCorretas.Contains(o.opcaoid);
                    await _repoOpcoes.AlterarAsync(o);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao definir gabarito: {ex.Message}");
            }
        }
    }
}
