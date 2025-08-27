using FASTSURVEY.Dtos.Blocklist;
using FASTSURVEY.Services.Blocklist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BlocklistController : BaseController
    {
        private readonly IBlocklistService _blocklistService;

        public BlocklistController(IBlocklistService blocklistService)
        {
            _blocklistService = blocklistService;
        }

        /// <summary>
        /// Verifica se um IP está bloqueado
        /// </summary>
        [HttpGet("status/{ipAddress}")]
        [AllowAnonymous]
        public async Task<ActionResult<BlocklistStatusResponse>> GetStatus(string ipAddress)
        {
            try
            {
                var status = await _blocklistService.GetStatusAsync(ipAddress);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao verificar status do IP", error = ex.Message }
                );
            }
        }

        /// <summary>
        /// Lista todos os IPs bloqueados
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BlocklistResponse>>> ListarTodos()
        {
            try
            {
                var blocklist = await _blocklistService.ListarTodosAsync();
                return Ok(blocklist);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao listar IPs bloqueados", error = ex.Message }
                );
            }
        }

        /// <summary>
        /// Adiciona um IP à blocklist
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BlocklistResponse>> Adicionar(
            [FromBody] BlocklistRequest request
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var blocklist = await _blocklistService.AddToBlocklistAsync(request);
                return CreatedAtAction(
                    nameof(GetStatus),
                    new { ipAddress = blocklist.IPAddress },
                    blocklist
                );
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao adicionar IP à blocklist", error = ex.Message }
                );
            }
        }

        /// <summary>
        /// Remove um IP da blocklist
        /// </summary>
        [HttpDelete("{ipAddress}")]
        public async Task<ActionResult> Remover(string ipAddress)
        {
            try
            {
                var removido = await _blocklistService.RemoveFromBlocklistAsync(ipAddress);

                if (!removido)
                    return NotFound(new { message = "IP não encontrado na blocklist" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao remover IP da blocklist", error = ex.Message }
                );
            }
        }

        /// <summary>
        /// Limpa entradas antigas da blocklist
        /// </summary>
        [HttpDelete("limpar-antigas")]
        public async Task<ActionResult> LimparAntigas([FromQuery] DateTime antesDe)
        {
            try
            {
                var removidos = await _blocklistService.LimparAntigasAsync(antesDe);
                return Ok(new { message = $"{removidos} entradas removidas da blocklist" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = "Erro ao limpar entradas antigas", error = ex.Message }
                );
            }
        }
    }
}
