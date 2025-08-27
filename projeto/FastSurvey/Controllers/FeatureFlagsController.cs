using FASTSURVEY.Dtos.FeatureFlags;
using FASTSURVEY.Services.FeatureFlags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class FeatureFlagsController : BaseController
    {
        private readonly IFeatureFlagsService _featureFlagsService;

        public FeatureFlagsController(IFeatureFlagsService featureFlagsService)
        {
            _featureFlagsService = featureFlagsService;
        }

        /// <summary>
        /// Verifica se uma feature flag está ativa para um usuário
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> GetStatus([FromQuery] int loginId, [FromQuery] string flag)
        {
            try
            {
                var isEnabled = await _featureFlagsService.IsFeatureEnabledAsync(loginId, flag);
                return Ok(isEnabled);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao verificar status da feature flag", error = ex.Message });
            }
        }

        /// <summary>
        /// Lista todas as feature flags de um usuário
        /// </summary>
        [HttpGet("usuario/{loginId}")]
        public async Task<ActionResult<List<FeatureFlagResponse>>> ListarPorUsuario(int loginId)
        {
            try
            {
                var featureFlags = await _featureFlagsService.ListarPorLoginAsync(loginId);
                return Ok(featureFlags);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao listar feature flags do usuário", error = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos os usuários que têm uma feature flag específica ativa
        /// </summary>
        [HttpGet("flag/{flag}")]
        public async Task<ActionResult<List<FeatureFlagResponse>>> ListarPorFlag(string flag)
        {
            try
            {
                var featureFlags = await _featureFlagsService.ListarPorFlagAsync(flag);
                return Ok(featureFlags);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao listar usuários com a feature flag", error = ex.Message });
            }
        }

        /// <summary>
        /// Define ou atualiza uma feature flag para um usuário
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FeatureFlagResponse>> Definir([FromBody] FeatureFlagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var featureFlag = await _featureFlagsService.SetFeatureFlagAsync(request);
                return Ok(featureFlag);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao definir feature flag", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove uma feature flag de um usuário
        /// </summary>
        [HttpDelete("{loginId}/{flag}")]
        public async Task<ActionResult> Remover(int loginId, string flag)
        {
            try
            {
                var removido = await _featureFlagsService.RemoveFeatureFlagAsync(loginId, flag);
                
                if (!removido)
                    return NotFound(new { message = "Feature flag não encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao remover feature flag", error = ex.Message });
            }
        }
    }
}
