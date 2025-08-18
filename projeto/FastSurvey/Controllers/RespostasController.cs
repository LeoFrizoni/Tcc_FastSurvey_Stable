using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using FASTSURVEY.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    // DTOs simples para entrada
    public class RespostaDiscursivaDto
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = "";
        public List<int> AnexosIds { get; set; } = new(); // opcional: anexos já existentes a vincular
    }

    public class RespostaOpcoesDto
    {
        public int PerguntaId { get; set; }
        public List<int> OpcoesSelecionadas { get; set; } = new(); // lista de opcaoid
        public List<int> AnexosIds { get; set; } = new();          // opcional
    }

    [Route("api/[controller]")]
    [ApiController]
    public class RespostasController : ControllerBase
    {
        private readonly ServiceRespostas _serviceRespostas;
        private readonly FastSurveyContext _context;

        public RespostasController(ServiceRespostas serviceRespostas, FastSurveyContext context)
        {
            _serviceRespostas = serviceRespostas;
            _context = context;
        }

        // =========================================================
        // POST: api/Respostas/discursiva
        // Cria resposta discursiva (texto) + vínculo de anexos (opcional)
        // =========================================================
        [HttpPost("discursiva")]
        public async Task<IActionResult> ResponderDiscursiva([FromBody] RespostaDiscursivaDto dto)
        {
            if (dto == null || dto.PerguntaId <= 0)
                return BadRequest("Payload inválido.");
            if (string.IsNullOrWhiteSpace(dto.Texto))
                return BadRequest("Texto da resposta é obrigatório para perguntas discursivas.");

            try
            {
                // Confirma que a pergunta existe e é da pesquisa
                var perguntaExiste = await _context.perguntas.AsNoTracking()
                    .AnyAsync(p => p.perguntaid == dto.PerguntaId);
                if (!perguntaExiste)
                    return NotFound("Pergunta não encontrada.");

                var resp = new respostas
                {
                    perguntaid = dto.PerguntaId,
                    texto = dto.Texto?.Trim() ?? "",
                    dataresposta = DateTime.UtcNow
                };

                // Você pode usar o service ou gravar direto pelo context
                // var criada = await _serviceRespostas.CadastrarRespostaAsync(resp);
                _context.respostas.Add(resp);
                await _context.SaveChangesAsync();
                var criada = resp;

                // Vincula anexos (se houver)
                if (dto.AnexosIds?.Count > 0)
                {
                    var pairs = dto.AnexosIds.Distinct().Select(id => new respostas_anexos
                    {
                        respostaid = criada.respostaid,
                        anexoid = id
                    });
                    await _context.respostas_anexos.AddRangeAsync(pairs);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(ObterResposta), new { id = criada.respostaid }, new
                {
                    criada.respostaid,
                    criada.perguntaid,
                    criada.texto,
                    criada.dataresposta
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar resposta discursiva: {ex.Message}");
            }
        }

        // =========================================================
        // POST: api/Respostas/opcoes
        // Cria resposta para objetiva/múltipla (vincula opções) + anexos (opcional)
        // =========================================================
        [HttpPost("opcoes")]
        public async Task<IActionResult> ResponderComOpcoes([FromBody] RespostaOpcoesDto dto)
        {
            if (dto == null || dto.PerguntaId <= 0)
                return BadRequest("Payload inválido.");
            if (dto.OpcoesSelecionadas == null || dto.OpcoesSelecionadas.Count == 0)
                return BadRequest("Selecione pelo menos uma opção.");

            try
            {
                // Certifica que a pergunta existe
                var pergunta = await _context.perguntas.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.perguntaid == dto.PerguntaId);
                if (pergunta == null)
                    return NotFound("Pergunta não encontrada.");

                // Valida se as opções pertencem à pergunta
                var opcoesValidas = await _context.opcoespergunta
                    .AsNoTracking()
                    .Where(o => o.perguntaid == dto.PerguntaId && dto.OpcoesSelecionadas.Contains(o.opcaoid))
                    .Select(o => o.opcaoid)
                    .ToListAsync();

                if (opcoesValidas.Count != dto.OpcoesSelecionadas.Distinct().Count())
                    return BadRequest("Há opções inválidas para esta pergunta.");

                var resp = new respostas
                {
                    perguntaid = dto.PerguntaId,
                    texto = "", // para objetivas/múltiplas, mantemos vazio
                    dataresposta = DateTime.UtcNow
                };

                _context.respostas.Add(resp);
                await _context.SaveChangesAsync();
                var criada = resp;

                // Vincula opções
                var vincs = opcoesValidas.Distinct().Select(opId => new respostas_opcoes
                {
                    respostaid = criada.respostaid,
                    opcaoid = opId
                });
                await _context.respostas_opcoes.AddRangeAsync(vincs);

                // Vincula anexos (opcional)
                if (dto.AnexosIds?.Count > 0)
                {
                    var pairs = dto.AnexosIds.Distinct().Select(id => new respostas_anexos
                    {
                        respostaid = criada.respostaid,
                        anexoid = id
                    });
                    await _context.respostas_anexos.AddRangeAsync(pairs);
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterResposta), new { id = criada.respostaid }, new
                {
                    criada.respostaid,
                    criada.perguntaid,
                    opcoesSelecionadas = opcoesValidas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar resposta: {ex.Message}");
            }
        }

        // =========================================================
        // POST: api/Respostas/{respostaId}/opcoes
        // Adiciona opções a uma resposta já cadastrada (útil em múltipla)
        // =========================================================
        [HttpPost("{respostaId}/opcoes")]
        public async Task<IActionResult> AdicionarOpcoesNaResposta(int respostaId, [FromBody] List<int> opcoesIds)
        {
            if (respostaId <= 0) return BadRequest("ID inválido.");
            if (opcoesIds == null || opcoesIds.Count == 0) return BadRequest("Nenhuma opção enviada.");

            try
            {
                var resposta = await _context.respostas.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.respostaid == respostaId);
                if (resposta == null) return NotFound("Resposta não encontrada.");

                var opcoesValidas = await _context.opcoespergunta
                    .AsNoTracking()
                    .Where(o => o.perguntaid == resposta.perguntaid && opcoesIds.Contains(o.opcaoid))
                    .Select(o => o.opcaoid)
                    .ToListAsync();

                if (opcoesValidas.Count == 0)
                    return BadRequest("Nenhuma opção válida para a pergunta desta resposta.");

                var vincs = opcoesValidas.Distinct().Select(opId => new respostas_opcoes
                {
                    respostaid = respostaId,
                    opcaoid = opId
                });

                await _context.respostas_opcoes.AddRangeAsync(vincs);
                await _context.SaveChangesAsync();

                return Ok(new { respostaId, adicionadas = opcoesValidas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao adicionar opções: {ex.Message}");
            }
        }

        // =========================================================
        // GET: api/Respostas/{id}
        // Retorna a resposta + opções/anexos vinculados
        // =========================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterResposta(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var resposta = await _context.respostas
                    .AsNoTracking()
                    .Where(r => r.respostaid == id)
                    .Select(r => new
                    {
                        r.respostaid,
                        r.perguntaid,
                        r.texto,
                        r.dataresposta,
                        opcoes = _context.respostas_opcoes
                            .Where(ro => ro.respostaid == r.respostaid)
                            .Select(ro => ro.opcaoid)
                            .ToList(),
                        anexos = _context.respostas_anexos
                            .Where(ra => ra.respostaid == r.respostaid)
                            .Select(ra => ra.anexoid)
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (resposta == null) return NotFound("Resposta não encontrada.");
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar resposta: {ex.Message}");
            }
        }

        // =========================================================
        // GET: api/Respostas/pesquisa/{pesquisaId}
        // Lista respostas de uma pesquisa (com alias amigáveis)
        // =========================================================
        [HttpGet("pesquisa/{pesquisaId}")]
        public async Task<IActionResult> ObterRespostasPorPesquisa(int pesquisaId)
        {
            if (pesquisaId <= 0) return BadRequest("ID da pesquisa inválido.");

            try
            {
                var respostas = await _context.respostas
                    .AsNoTracking()
                    .Join(_context.perguntas,
                          r => r.perguntaid,
                          p => p.perguntaid,
                          (r, p) => new { r, p })
                    .Where(x => x.p.pesquisaid == pesquisaId)
                    .Select(x => new
                    {
                        respostaid = x.r.respostaid,
                        perguntaid = x.r.perguntaid,
                        perguntaTexto = x.p.texto,
                        tipoperguntaid = x.p.tipoperguntaid,
                        respostaTexto = x.r.texto,
                        dataresposta = x.r.dataresposta,
                        opcoes = _context.respostas_opcoes
                            .Where(ro => ro.respostaid == x.r.respostaid)
                            .Select(ro => ro.opcaoid)
                            .ToList(),
                        anexos = _context.respostas_anexos
                            .Where(ra => ra.respostaid == x.r.respostaid)
                            .Select(ra => ra.anexoid)
                            .ToList()
                    })
                    .ToListAsync();

                if (respostas == null || respostas.Count == 0)
                    return NotFound("Nenhuma resposta encontrada para a pesquisa especificada.");

                return Ok(respostas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar respostas por pesquisa: {ex.Message}");
            }
        }

        // =========================================================
        // PUT: api/Respostas/{id}
        // Atualiza apenas o texto da resposta (para objetivas/múltiplas, use endpoints de opções)
        // =========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarResposta(int id, [FromBody] respostas resposta)
        {
            if (id <= 0 || resposta == null || id != resposta.respostaid)
                return BadRequest("Dados inválidos.");

            try
            {
                var existente = await _context.respostas.FindAsync(id);
                if (existente == null) return NotFound("Resposta não encontrada.");

                existente.texto = resposta.texto ?? "";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    existente.respostaid,
                    existente.perguntaid,
                    existente.texto,
                    existente.dataresposta
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar resposta: {ex.Message}");
            }
        }

        // =========================================================
        // DELETE: api/Respostas/{id}
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirResposta(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var resposta = await _context.respostas.FindAsync(id);
                if (resposta == null) return NotFound("Resposta não encontrada.");

                _context.respostas.Remove(resposta);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir resposta: {ex.Message}");
            }
        }
    }
}
