using FASTSURVEY.Dtos.ExternalLogins;
using FASTSURVEY.Services.ExternalLogins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExternalLoginsController : BaseController
    {
        private readonly IExternalLoginsService _externalLoginsService;

        public ExternalLoginsController(IExternalLoginsService externalLoginsService)
        {
            _externalLoginsService = externalLoginsService;
        }

        /// <summary>
        /// Obtém login externo por provedor e ID do usuário no provedor
        /// </summary>
        [HttpGet("provedor/{provider}/usuario/{providerUserId}")]
        public async Task<ActionResult<ExternalLoginResponse>> GetByProvider(string provider, string providerUserId)
        {
            try
            {
                var externalLogin = await _externalLoginsService.GetByProviderAsync(provider, providerUserId);
                
                if (externalLogin == null)
                    return NotFound(new { message = "Login externo não encontrado" });

                return Ok(externalLogin);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar login externo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém login externo por ID do usuário
        /// </summary>
        [HttpGet("usuario/{loginId}")]
        public async Task<ActionResult<ExternalLoginResponse>> GetByLoginId(int loginId)
        {
            try
            {
                var externalLogin = await _externalLoginsService.GetByLoginIdAsync(loginId);
                
                if (externalLogin == null)
                    return NotFound(new { message = "Login externo não encontrado" });

                return Ok(externalLogin);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao buscar login externo", error = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos os logins externos de um usuário
        /// </summary>
        [HttpGet("usuario/{loginId}/todos")]
        public async Task<ActionResult<List<ExternalLoginResponse>>> GetAllByLoginId(int loginId)
        {
            try
            {
                var externalLogins = await _externalLoginsService.GetAllByLoginIdAsync(loginId);
                return Ok(externalLogins);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao listar logins externos", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se existe um login externo
        /// </summary>
        [HttpGet("existe")]
        public async Task<ActionResult<bool>> Exists([FromQuery] string provider, [FromQuery] string providerUserId)
        {
            try
            {
                var exists = await _externalLoginsService.ExistsAsync(provider, providerUserId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao verificar existência do login externo", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se um usuário tem login externo para um provedor específico
        /// </summary>
        [HttpGet("usuario/{loginId}/provedor/{provider}/existe")]
        public async Task<ActionResult<bool>> ExistsForLogin(int loginId, string provider)
        {
            try
            {
                var exists = await _externalLoginsService.ExistsForLoginAsync(loginId, provider);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao verificar existência do login externo", error = ex.Message });
            }
        }

        /// <summary>
        /// Cria um novo login externo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ExternalLoginResponse>> Criar([FromBody] ExternalLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var externalLogin = await _externalLoginsService.CreateAsync(request);
                return CreatedAtAction(nameof(GetByProvider), 
                    new { provider = externalLogin.Provider, providerUserId = externalLogin.ProviderUserId }, 
                    externalLogin);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao criar login externo", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove um login externo
        /// </summary>
        [HttpDelete("{externalLoginId}")]
        public async Task<ActionResult> Remover(int externalLoginId)
        {
            try
            {
                var removido = await _externalLoginsService.DeleteAsync(externalLoginId);
                
                if (!removido)
                    return NotFound(new { message = "Login externo não encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao remover login externo", error = ex.Message });
            }
        }
    }
}
