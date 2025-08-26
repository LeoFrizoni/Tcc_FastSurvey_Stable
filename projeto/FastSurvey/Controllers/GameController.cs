using FASTSURVEY.Services.PesquisaInterativa;
using FASTSURVEY.Services.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// using Microsoft.AspNetCore.RateLimiting; // se usar rate limiting
// using Microsoft.Net.Http.Headers;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GameController : ControllerBase
    {
        private readonly IGamificationService _gameService;

        public GameController(IGamificationService gameService)
        {
            _gameService = gameService;
        }

        // POST: /api/game/create
        [HttpPost("create")]
        [Authorize] // host precisa estar autenticado
        public async Task<IActionResult> CreateGameSession(
            [FromBody] CreateGameSessionRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.CreateGameSessionAsync(request, ct);
            if (!result.Success)
                return MapFail(result);

            // 201 Created com Location apontando para o status da sessão
            var sessionId = result.Data!.SessionId;
            return Created(
                Url.Action(nameof(GetGameStatus), new { sessionId })!,
                new { success = true, data = result.Data }
            );
        }

        // POST: /api/game/join
        [HttpPost("join")]
        [AllowAnonymous]
        // [EnableRateLimiting("game")] // opcional
        public async Task<IActionResult> JoinGame(
            [FromBody] JoinGameRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.JoinGameAsync(request, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // GET: /api/game/join/{accessCode}
        [HttpGet("join/{accessCode}")]
        [AllowAnonymous]
        public IActionResult JoinGameByCode([FromRoute] string accessCode)
        {
            return Redirect($"/game/join?code={accessCode}");
        }

        // GET: /api/game/{sessionId}/question
        [HttpGet("{sessionId}/question")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetCurrentQuestion(
            [FromRoute] string sessionId,
            CancellationToken ct
        )
        {
            var result = await _gameService.GetCurrentQuestionAsync(sessionId, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // POST: /api/game/answer
        [HttpPost("answer")]
        [AllowAnonymous]
        // [EnableRateLimiting("game")]
        public async Task<IActionResult> SubmitAnswer(
            [FromBody] SubmitAnswerRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.SubmitAnswerAsync(request, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // GET: /api/game/{sessionId}/leaderboard
        [HttpGet("{sessionId}/leaderboard")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetLeaderboard(
            [FromRoute] string sessionId,
            CancellationToken ct
        )
        {
            var result = await _gameService.GetLeaderboardAsync(sessionId, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // GET: /api/game/{sessionId}/stats
        [HttpGet("{sessionId}/stats")]
        [Authorize] // stats completos só para host/admin
        public async Task<IActionResult> GetGameStats(
            [FromRoute] string sessionId,
            CancellationToken ct
        )
        {
            var result = await _gameService.GetGameStatsAsync(sessionId, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // POST: /api/game/{sessionId}/start-timer
        [HttpPost("{sessionId}/start-timer")]
        [Authorize]
        public async Task<IActionResult> StartQuestionTimer(
            [FromRoute] string sessionId,
            [FromBody] StartTimerRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.StartQuestionTimerAsync(
                sessionId,
                request.QuestionId,
                request.TimeLimit,
                ct
            );
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // POST: /api/game/{sessionId}/end-question
        [HttpPost("{sessionId}/end-question")]
        [Authorize]
        public async Task<IActionResult> EndQuestion(
            [FromRoute] string sessionId,
            [FromBody] EndQuestionRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.EndQuestionAsync(sessionId, request.QuestionId, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // PUT: /api/game/{sessionId}/settings
        [HttpPut("{sessionId}/settings")]
        [Authorize]
        public async Task<IActionResult> UpdateGameSettings(
            [FromRoute] string sessionId,
            [FromBody] GameSettingsRequest request,
            CancellationToken ct
        )
        {
            var result = await _gameService.UpdateGameSettingsAsync(sessionId, request, ct);
            return result.Success
                ? Ok(new { success = true, data = result.Data })
                : MapFail(result);
        }

        // GET: /api/game/{sessionId}/status
        [HttpGet("{sessionId}/status")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetGameStatus(
            [FromRoute] string sessionId,
            CancellationToken ct
        )
        {
            var statsResult = await _gameService.GetGameStatsAsync(sessionId, ct);
            if (!statsResult.Success)
                return MapFail(statsResult);

            var leaderboardResult = await _gameService.GetLeaderboardAsync(sessionId, ct);

            return Ok(
                new
                {
                    success = true,
                    data = new
                    {
                        stats = statsResult.Data,
                        leaderboard = leaderboardResult.Success ? leaderboardResult.Data : null,
                        timestamp = DateTime.UtcNow,
                    },
                }
            );
        }

        // POST: /api/game/{sessionId}/powerup
        [HttpPost("{sessionId}/powerup")]
        [AllowAnonymous]
        public IActionResult UsePowerUp(
            [FromRoute] string sessionId,
            [FromBody] UsePowerUpRequest request
        )
        {
            // Quando integrar a lógica de power-ups no service, chamar lá e mapear o retorno
            return StatusCode(
                StatusCodes.Status501NotImplemented,
                new
                {
                    success = false,
                    error = "NOT_IMPLEMENTED",
                    message = "Uso de power-ups ainda não implementado no servidor.",
                }
            );
        }

        // GET: /api/game/{sessionId}/results
        [HttpGet("{sessionId}/results")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetFinalResults(
            [FromRoute] string sessionId,
            CancellationToken ct
        )
        {
            var statsResult = await _gameService.GetGameStatsAsync(sessionId, ct);
            if (!statsResult.Success)
                return MapFail(statsResult);

            var leaderboardResult = await _gameService.GetLeaderboardAsync(sessionId, ct);

            return Ok(
                new
                {
                    success = true,
                    data = new
                    {
                        finalStats = statsResult.Data,
                        finalLeaderboard = leaderboardResult.Success
                            ? leaderboardResult.Data
                            : null,
                        gameEndedAt = DateTime.UtcNow,
                        winners = leaderboardResult.Success
                            ? leaderboardResult.Data!.Players.Take(3).ToList()
                            : new List<LeaderboardEntry>(),
                    },
                }
            );
        }

        // ===== Helpers =====
        // ... dentro do GameController

        private IActionResult MapFail<T>(ServiceResult<T> result)
        {
            var code = result.Code ?? "ERROR";
            var payload = new
            {
                success = false,
                error = code,
                message = result.Message,
            };

            return code switch
            {
                "NOT_FOUND" => NotFound(payload),
                "NOT_INTERACTIVE" => Conflict(payload),
                "SESSION_ENDED" => StatusCode(StatusCodes.Status410Gone, payload),
                "LATE_JOINING_DISABLED" => StatusCode(StatusCodes.Status403Forbidden, payload),
                "MAX_PLAYERS_REACHED" => Conflict(payload),
                "WRONG_QUESTION" => Conflict(payload),
                "NO_MORE_QUESTIONS" => StatusCode(StatusCodes.Status410Gone, payload),
                _ => BadRequest(payload),
            };
        }
    }

    // DTOs do controller
    public class StartTimerRequest
    {
        public int QuestionId { get; set; }
        public int TimeLimit { get; set; } = 30;
    }

    public class EndQuestionRequest
    {
        public int QuestionId { get; set; }
    }

    public class UsePowerUpRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PowerUpId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
    }
}
