using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class OpcoesPerguntaController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryOpcoesPergunta _repositoryOpcoesPergunta;

        public OpcoesPerguntaController(FastSurveyContext context)
        {
            _context = context;
            _repositoryOpcoesPergunta = new RepositoryOpcoesPergunta(_context);
        }

        // DTOs simples (evita expor a entidade diretamente)
        public class OpcaoCreateDto
        {
            public int perguntaid { get; set; }
            public string texto { get; set; } = "";
            public bool? correta { get; set; } // opcional
        }

        public class OpcaoUpdateDto
        {
            public int opcaoid { get; set; }
            public string? texto { get; set; }
            public bool? correta { get; set; } // opcional
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OpcaoCreateDto dto)
        {
            if (dto == null || dto.perguntaid <= 0 || string.IsNullOrWhiteSpace(dto.texto))
                return BadRequest("Dados inválidos para a opção.");

            var texto = dto.texto.Trim();
            if (texto.Length > 200)
                return BadRequest("O texto da opção não pode exceder 200 caracteres.");

            try
            {
                // pergunta precisa existir
                var pergunta = await _context.perguntas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.perguntaid == dto.perguntaid);

                if (pergunta == null)
                    return NotFound("Pergunta não encontrada.");

                // checa duplicidade (case-insensitive)
                var jaExiste = await _context.opcoespergunta
                    .AsNoTracking()
                    .AnyAsync(o => o.perguntaid == dto.perguntaid &&
                                   EF.Functions.ILike(o.texto, texto));
                if (jaExiste)
                    return Conflict("Já existe uma opção com esse texto para esta pergunta.");

                var nova = new opcoespergunta
                {
                    perguntaid = dto.perguntaid,
                    texto = texto,
                    correta = dto.correta ?? false
                };

                await _repositoryOpcoesPergunta.IncluirAsync(nova);

                // se marcou correta e a pergunta NÃO permite múltipla, zera as demais
                if (nova.correta && !pergunta.permitemultiplaselecao)
                {
                    await _context.opcoespergunta
                        .Where(o => o.perguntaid == nova.perguntaid && o.opcaoid != nova.opcaoid && o.correta)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.correta, false));
                }

                return CreatedAtAction(nameof(GetPorPergunta),
                    new { perguntaId = nova.perguntaid },
                    new { nova.opcaoid, nova.perguntaid, nova.texto, nova.correta });
            }
            catch (DbUpdateException ex)
            {
                // cobre violação do índice único (perguntaid, texto)
                return Conflict($"Conflito ao salvar opção: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar opção: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var opcoes = await _context.opcoespergunta
                    .AsNoTracking()
                    .OrderBy(o => o.perguntaid).ThenBy(o => o.opcaoid)
                    .ToListAsync();

                if (opcoes.Count == 0)
                    return NotFound("Nenhuma opção encontrada.");

                return Ok(opcoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar opções: {ex.Message}");
            }
        }

        [HttpGet("Pergunta/{perguntaId:int}")]
        public async Task<IActionResult> GetPorPergunta(int perguntaId)
        {
            if (perguntaId <= 0)
                return BadRequest("ID de pergunta inválido.");

            try
            {
                var opcoes = await _context.opcoespergunta
                    .AsNoTracking()
                    .Where(o => o.perguntaid == perguntaId)
                    .OrderBy(o => o.opcaoid)
                    .ToListAsync();

                // aqui eu devolvo [] se vazio, mas se preferir 404, troque a linha abaixo
                return Ok(opcoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar opções: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] OpcaoUpdateDto dto)
        {
            if (id <= 0 || dto == null || id != dto.opcaoid)
                return BadRequest("Dados inválidos para atualização.");

            try
            {
                var existente = await _context.opcoespergunta
                    .FirstOrDefaultAsync(o => o.opcaoid == id);

                if (existente == null)
                    return NotFound("Opção não encontrada.");

                // Atualiza texto (com validações)
                if (!string.IsNullOrWhiteSpace(dto.texto))
                {
                    var novoTexto = dto.texto.Trim();
                    if (novoTexto.Length > 200)
                        return BadRequest("O texto da opção não pode exceder 200 caracteres.");

                    var conflito = await _context.opcoespergunta
                        .AsNoTracking()
                        .AnyAsync(o => o.perguntaid == existente.perguntaid &&
                                       o.opcaoid != existente.opcaoid &&
                                       EF.Functions.ILike(o.texto, novoTexto));
                    if (conflito)
                        return Conflict("Já existe uma opção com esse texto para esta pergunta.");

                    existente.texto = novoTexto;
                }

                // Atualiza flag 'correta' (se informada)
                if (dto.correta.HasValue && existente.correta != dto.correta.Value)
                {
                    existente.correta = dto.correta.Value;

                    if (existente.correta)
                    {
                        // se a pergunta não permite múltiplas, desmarca todas as outras
                        var pergunta = await _context.perguntas
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.perguntaid == existente.perguntaid);

                        if (pergunta != null && !pergunta.permitemultiplaselecao)
                        {
                            await _context.opcoespergunta
                                .Where(o => o.perguntaid == existente.perguntaid &&
                                            o.opcaoid != existente.opcaoid &&
                                            o.correta)
                                .ExecuteUpdateAsync(s => s.SetProperty(x => x.correta, false));
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Conflito ao atualizar opção: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar opção: {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("ID inválido.");

            try
            {
                var opcao = await _repositoryOpcoesPergunta.SelecionarChaveAsync(id);
                if (opcao == null)
                    return NotFound("Opção não encontrada.");

                await _repositoryOpcoesPergunta.ExcluirAsync(opcao);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir opção: {ex.Message}");
            }
        }
    }
}
