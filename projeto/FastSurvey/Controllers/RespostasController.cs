using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Resposta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RespostasController : ControllerBase
    {
        private readonly IRespostaService _svc;

        public RespostasController(IRespostaService svc) => _svc = svc;

        [HttpPost("discursiva")]
        public async Task<IActionResult> CriarDiscursiva([FromBody] CriarRespostaDiscursivaRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var entity = await _svc.CriarDiscursivaAsync(req, ct);

                var dto = new RespostaDto
                {
                    RespostaId = entity.Respostaid,
                    PerguntaId = entity.Perguntaid,
                    Texto = entity.Texto,
                    DataResposta = entity.Dataresposta,
                    Opcoes = new()
                };

                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Erro ao salvar resposta no banco." });
            }
            catch
            {
                return StatusCode(500, new { error = "Erro inesperado ao criar resposta." });
            }
        }

        [HttpPost("opcoes")]
        public async Task<IActionResult> CriarOpcoes([FromBody] CriarRespostaOpcoesRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var entity = await _svc.CriarOpcoesAsync(req, ct);

                var dto = new RespostaDto
                {
                    RespostaId = entity.Respostaid,
                    PerguntaId = entity.Perguntaid,
                    Texto = entity.Texto,
                    DataResposta = entity.Dataresposta,
                    Opcoes = entity.Opcao.Select(o => o.Opcaoid).ToList()
                };

                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Erro ao salvar resposta no banco." });
            }
            catch
            {
                return StatusCode(500, new { error = "Erro inesperado ao criar resposta." });
            }
        }
    }
}
