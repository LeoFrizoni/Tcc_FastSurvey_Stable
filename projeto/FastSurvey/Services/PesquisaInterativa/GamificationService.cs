using System.Text.Json;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public class GamificationService : IGamificationService
    {
        private readonly FastSurveyContext _ctx;
        private static readonly Random _random = new();
        private static readonly Dictionary<string, GameSession> _activeSessions = new();
        private static readonly Dictionary<string, Dictionary<string, PlayerData>> _playerSessions =
            new();

        public GamificationService(FastSurveyContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ServiceResult<GameSessionResponse>> CreateGameSessionAsync(
            CreateGameSessionRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .FirstOrDefaultAsync(p => p.PesquisaId == request.PesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<GameSessionResponse>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                if (!pesquisa.IsInterativa)
                    return ServiceResult<GameSessionResponse>.Fail(
                        "NOT_INTERACTIVE",
                        "Esta pesquisa não é interativa"
                    );

                var sessionId = Guid.NewGuid().ToString("N");
                var accessCode = GenerateAccessCode();

                var gameSession = new GameSession
                {
                    SessionId = sessionId,
                    AccessCode = accessCode,
                    PesquisaId = request.PesquisaId,
                    HostName = request.HostName,
                    Status = GameStatus.Waiting,
                    Settings = request.Settings,
                    CreatedAt = DateTime.UtcNow,
                    CurrentQuestionIndex = 0,
                    Players = new Dictionary<string, PlayerData>(),
                    Questions = pesquisa
                        .Perguntas.OrderBy(p => p.Ordem)
                        .Select(p => new GameQuestion
                        {
                            QuestionId = p.PerguntaId,
                            Text = p.Texto,
                            Type = (QuestionType)p.TipoPerguntaId,
                            Order = p.Ordem,
                            Options = p
                                .OpcoesPergunta.OrderBy(o => o.Ordem)
                                .Select(o => new GameOption
                                {
                                    OptionId = o.OpcaoId,
                                    Text = o.Texto,
                                    Order = o.Ordem,
                                    IsCorrect = o.Correta,
                                })
                                .ToList(),
                        })
                        .ToList(),
                };

                _activeSessions[sessionId] = gameSession;
                _playerSessions[sessionId] = new Dictionary<string, PlayerData>();

                // Gerar QR Code URL
                var qrCodeUrl = $"/api/game/join/{accessCode}";

                return ServiceResult<GameSessionResponse>.Ok(
                    new GameSessionResponse
                    {
                        SessionId = sessionId,
                        AccessCode = accessCode,
                        HostName = request.HostName,
                        Status = GameStatus.Waiting,
                        Settings = request.Settings,
                        CreatedAt = gameSession.CreatedAt,
                        TotalPlayers = 0,
                        CurrentQuestionIndex = 0,
                        QrCodeUrl = qrCodeUrl,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<GameSessionResponse>.Fail(
                    "ERROR",
                    $"Erro ao criar sessão: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<PlayerResponse>> JoinGameAsync(
            JoinGameRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var session = _activeSessions.Values.FirstOrDefault(s =>
                    s.AccessCode == request.AccessCode
                );
                if (session == null)
                    return ServiceResult<PlayerResponse>.Fail("NOT_FOUND", "Sessão não encontrada");

                if (session.Status == GameStatus.Finished || session.Status == GameStatus.Cancelled)
                    return ServiceResult<PlayerResponse>.Fail(
                        "SESSION_ENDED",
                        "Sessão já foi finalizada"
                    );

                if (!session.Settings.AllowLateJoining && session.Status == GameStatus.Active)
                    return ServiceResult<PlayerResponse>.Fail(
                        "LATE_JOINING_DISABLED",
                        "Entrada tardia não permitida"
                    );

                if (session.Players.Count >= session.Settings.MaxPlayers)
                    return ServiceResult<PlayerResponse>.Fail(
                        "MAX_PLAYERS_REACHED",
                        "Número máximo de jogadores atingido"
                    );

                var playerId = Guid.NewGuid().ToString("N");
                var playerData = new PlayerData
                {
                    PlayerId = playerId,
                    PlayerName = request.PlayerName,
                    TeamName = request.TeamName,
                    AvatarUrl = request.AvatarUrl,
                    Score = 0,
                    Streak = 0,
                    Status = PlayerStatus.Connected,
                    JoinedAt = DateTime.UtcNow,
                    CorrectAnswers = 0,
                    TotalAnswers = 0,
                    ResponseTimes = new List<int>(),
                };

                session.Players[playerId] = playerData;
                _playerSessions[session.SessionId][playerId] = playerData;

                return ServiceResult<PlayerResponse>.Ok(
                    new PlayerResponse
                    {
                        PlayerId = playerId,
                        PlayerName = request.PlayerName,
                        TeamName = request.TeamName,
                        AvatarUrl = request.AvatarUrl,
                        Score = 0,
                        Streak = 0,
                        Status = PlayerStatus.Connected,
                        JoinedAt = playerData.JoinedAt,
                        ActivePowerUps = new List<PowerUp>(),
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<PlayerResponse>.Fail(
                    "ERROR",
                    $"Erro ao entrar no jogo: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<QuestionResponse>> GetCurrentQuestionAsync(
            string sessionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<QuestionResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                if (session.CurrentQuestionIndex >= session.Questions.Count)
                    return ServiceResult<QuestionResponse>.Fail(
                        "NO_MORE_QUESTIONS",
                        "Não há mais perguntas"
                    );

                var question = session.Questions[session.CurrentQuestionIndex];
                var timeLimit = session.Settings.DefaultTimeLimit;

                return ServiceResult<QuestionResponse>.Ok(
                    new QuestionResponse
                    {
                        QuestionId = question.QuestionId,
                        Text = question.Text,
                        Type = question.Type,
                        TimeLimit = timeLimit,
                        Order = question.Order,
                        Options = question
                            .Options.Select(o => new OptionResponse
                            {
                                OptionId = o.OptionId,
                                Text = o.Text,
                                Order = o.Order,
                                IsCorrect = o.IsCorrect,
                                Color = GetOptionColor(o.Order),
                                Icon = GetOptionIcon(o.Order),
                            })
                            .ToList(),
                        IsActive = session.Status == GameStatus.Active,
                        StartedAt = question.StartedAt,
                        EndsAt = question.EndsAt,
                        Media = question.Media,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<QuestionResponse>.Fail(
                    "ERROR",
                    $"Erro ao obter pergunta: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<AnswerResponse>> SubmitAnswerAsync(
            SubmitAnswerRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(request.SessionId, out var session))
                    return ServiceResult<AnswerResponse>.Fail("NOT_FOUND", "Sessão não encontrada");

                if (!session.Players.TryGetValue(request.PlayerId, out var player))
                    return ServiceResult<AnswerResponse>.Fail(
                        "PLAYER_NOT_FOUND",
                        "Jogador não encontrado"
                    );

                var currentQuestion = session.Questions[session.CurrentQuestionIndex];
                if (currentQuestion.QuestionId != request.QuestionId)
                    return ServiceResult<AnswerResponse>.Fail(
                        "WRONG_QUESTION",
                        "Pergunta incorreta"
                    );

                // Verificar se a resposta está correta
                var isCorrect = CheckAnswer(currentQuestion, request);
                var pointsEarned = CalculatePoints(
                    isCorrect,
                    request.TimeSpent,
                    player.Streak,
                    session.Settings
                );
                var streakBonus = CalculateStreakBonus(isCorrect, player.Streak);
                var timeBonus = CalculateTimeBonus(
                    request.TimeSpent,
                    session.Settings.DefaultTimeLimit
                );

                // Atualizar dados do jogador
                player.Score += pointsEarned;
                player.TotalAnswers++;
                player.ResponseTimes.Add(request.TimeSpent);

                if (isCorrect)
                {
                    player.Streak++;
                    player.CorrectAnswers++;
                }
                else
                {
                    player.Streak = 0;
                }

                // Registrar resposta
                var answer = new GameAnswer
                {
                    PlayerId = request.PlayerId,
                    QuestionId = request.QuestionId,
                    SelectedOptions = request.SelectedOptions,
                    TextAnswer = request.TextAnswer,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    TimeSpent = request.TimeSpent,
                    SubmittedAt = request.SubmittedAt,
                };

                currentQuestion.Answers.Add(answer);

                // Verificar power-ups
                var earnedPowerUps = CheckPowerUps(player, isCorrect, pointsEarned);

                return ServiceResult<AnswerResponse>.Ok(
                    new AnswerResponse
                    {
                        IsCorrect = isCorrect,
                        PointsEarned = pointsEarned,
                        StreakBonus = streakBonus,
                        TimeBonus = timeBonus,
                        TotalPoints = pointsEarned + streakBonus + timeBonus,
                        Message = GetAnswerMessage(isCorrect, pointsEarned, player.Streak),
                        EarnedPowerUps = earnedPowerUps,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<AnswerResponse>.Fail(
                    "ERROR",
                    $"Erro ao submeter resposta: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<LeaderboardResponse>> GetLeaderboardAsync(
            string sessionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<LeaderboardResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                var players = session
                    .Players.Values.Where(p => p.Status == PlayerStatus.Connected)
                    .OrderByDescending(p => p.Score)
                    .ThenByDescending(p => p.CorrectAnswers)
                    .ThenBy(p => p.JoinedAt)
                    .Select(
                        (p, index) =>
                            new LeaderboardEntry
                            {
                                Id = p.PlayerId,
                                Name = p.PlayerName,
                                Score = p.Score,
                                Position = index + 1,
                                Streak = p.Streak,
                                CorrectAnswers = p.CorrectAnswers,
                                TotalAnswers = p.TotalAnswers,
                                Accuracy =
                                    p.TotalAnswers > 0
                                        ? (double)p.CorrectAnswers / p.TotalAnswers * 100
                                        : 0,
                                AvatarUrl = p.AvatarUrl,
                                TeamName = p.TeamName,
                            }
                    )
                    .ToList();

                var teams = session
                    .Players.Values.Where(p => !string.IsNullOrEmpty(p.TeamName))
                    .GroupBy(p => p.TeamName)
                    .Select(g => new LeaderboardEntry
                    {
                        Id = g.Key!,
                        Name = g.Key!,
                        Score = g.Sum(p => p.Score),
                        Position = 0, // Será calculado depois
                        Streak = g.Max(p => p.Streak),
                        CorrectAnswers = g.Sum(p => p.CorrectAnswers),
                        TotalAnswers = g.Sum(p => p.TotalAnswers),
                        Accuracy =
                            g.Sum(p => p.TotalAnswers) > 0
                                ? (double)g.Sum(p => p.CorrectAnswers)
                                    / g.Sum(p => p.TotalAnswers)
                                    * 100
                                : 0,
                        TeamName = g.Key,
                    })
                    .OrderByDescending(t => t.Score)
                    .ToList();

                // Recalcular posições das equipes
                for (int i = 0; i < teams.Count; i++)
                {
                    teams[i].Position = i + 1;
                }

                return ServiceResult<LeaderboardResponse>.Ok(
                    new LeaderboardResponse
                    {
                        SessionId = sessionId,
                        Players = players,
                        Teams = teams,
                        LastUpdated = DateTime.UtcNow,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<LeaderboardResponse>.Fail(
                    "ERROR",
                    $"Erro ao obter leaderboard: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<GameStatsResponse>> GetGameStatsAsync(
            string sessionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<GameStatsResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                var players = session.Players.Values;
                var activePlayers = players.Count(p => p.Status == PlayerStatus.Connected);
                var averageScore = players.Any() ? players.Average(p => p.Score) : 0;
                var highestScore = players.Any() ? players.Max(p => p.Score) : 0;
                var highestScorer = players
                    .FirstOrDefault(p => p.Score == highestScore)
                    ?.PlayerName;

                var allResponseTimes = players.SelectMany(p => p.ResponseTimes);
                var averageResponseTime = allResponseTimes.Any()
                    ? TimeSpan.FromSeconds(allResponseTimes.Average())
                    : TimeSpan.Zero;

                var questionStats = session.Questions.ToDictionary(
                    q => q.QuestionId.ToString(),
                    q => q.Answers.Count(a => a.IsCorrect)
                );

                return ServiceResult<GameStatsResponse>.Ok(
                    new GameStatsResponse
                    {
                        SessionId = sessionId,
                        TotalPlayers = players.Count,
                        ActivePlayers = activePlayers,
                        TotalQuestions = session.Questions.Count,
                        CompletedQuestions = session.CurrentQuestionIndex,
                        AverageScore = averageScore,
                        HighestScore = highestScore,
                        HighestScorer = highestScorer,
                        AverageResponseTime = averageResponseTime,
                        QuestionStats = questionStats,
                        SessionStart = session.CreatedAt,
                        SessionEnd = session.Status == GameStatus.Finished ? DateTime.UtcNow : null,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<GameStatsResponse>.Fail(
                    "ERROR",
                    $"Erro ao obter estatísticas: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> StartQuestionTimerAsync(
            string sessionId,
            int questionId,
            int timeLimit,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Sessão não encontrada");

                var question = session.Questions[session.CurrentQuestionIndex];
                if (question.QuestionId != questionId)
                    return ServiceResult<bool>.Fail("WRONG_QUESTION", "Pergunta incorreta");

                question.StartedAt = DateTime.UtcNow;
                question.EndsAt = question.StartedAt.Value.AddSeconds(timeLimit);
                session.Status = GameStatus.Active;

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao iniciar timer: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> EndQuestionAsync(
            string sessionId,
            int questionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Sessão não encontrada");

                var question = session.Questions[session.CurrentQuestionIndex];
                if (question.QuestionId != questionId)
                    return ServiceResult<bool>.Fail("WRONG_QUESTION", "Pergunta incorreta");

                question.EndsAt = DateTime.UtcNow;
                session.CurrentQuestionIndex++;

                if (session.CurrentQuestionIndex >= session.Questions.Count)
                {
                    session.Status = GameStatus.Finished;
                }
                else
                {
                    session.Status = GameStatus.Waiting;
                }

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao finalizar pergunta: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<GameSettingsResponse>> UpdateGameSettingsAsync(
            string sessionId,
            GameSettingsRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                    return ServiceResult<GameSettingsResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                // Atualizar configurações
                if (request.EnableTimer.HasValue)
                    session.Settings.EnableTimer = request.EnableTimer.Value;
                if (request.DefaultTimeLimit.HasValue)
                    session.Settings.DefaultTimeLimit = request.DefaultTimeLimit.Value;
                if (request.ShowLeaderboard.HasValue)
                    session.Settings.ShowLeaderboard = request.ShowLeaderboard.Value;
                if (request.EnableSound.HasValue)
                    session.Settings.EnableSound = request.EnableSound.Value;
                if (request.EnableAnimations.HasValue)
                    session.Settings.EnableAnimations = request.EnableAnimations.Value;
                if (request.AllowLateJoining.HasValue)
                    session.Settings.AllowLateJoining = request.AllowLateJoining.Value;
                if (request.MaxPlayers.HasValue)
                    session.Settings.MaxPlayers = request.MaxPlayers.Value;
                if (request.EnableTeamMode.HasValue)
                    session.Settings.EnableTeamMode = request.EnableTeamMode.Value;
                if (request.ShowCorrectAnswers.HasValue)
                    session.Settings.ShowCorrectAnswers = request.ShowCorrectAnswers.Value;
                if (request.EnablePowerUps.HasValue)
                    session.Settings.EnablePowerUps = request.EnablePowerUps.Value;

                return ServiceResult<GameSettingsResponse>.Ok(
                    new GameSettingsResponse
                    {
                        SessionId = sessionId,
                        Settings = session.Settings,
                        Success = true,
                        Message = "Configurações atualizadas com sucesso",
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<GameSettingsResponse>.Fail(
                    "ERROR",
                    $"Erro ao atualizar configurações: {ex.Message}"
                );
            }
        }

        #region Private Methods

        private string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(
                Enumerable.Repeat(chars, 6).Select(s => s[_random.Next(s.Length)]).ToArray()
            );
        }

        private bool CheckAnswer(GameQuestion question, SubmitAnswerRequest request)
        {
            if (question.Type == QuestionType.Text)
            {
                // Para perguntas de texto, verificar se há resposta
                return !string.IsNullOrWhiteSpace(request.TextAnswer);
            }

            var correctOptions = question
                .Options.Where(o => o.IsCorrect)
                .Select(o => o.OptionId)
                .ToList();
            var selectedOptions = request.SelectedOptions;

            if (correctOptions.Count != selectedOptions.Count)
                return false;

            return correctOptions.All(option => selectedOptions.Contains(option));
        }

        private int CalculatePoints(
            bool isCorrect,
            int timeSpent,
            int streak,
            GameSettings settings
        )
        {
            if (!isCorrect)
                return 0;

            var basePoints = 100;
            var timeBonus = Math.Max(0, settings.DefaultTimeLimit - timeSpent) * 2;
            var streakBonus = streak * 10;

            return basePoints + timeBonus + streakBonus;
        }

        private int CalculateStreakBonus(bool isCorrect, int currentStreak)
        {
            if (!isCorrect)
                return 0;
            return currentStreak * 5;
        }

        private int CalculateTimeBonus(int timeSpent, int timeLimit)
        {
            var remainingTime = Math.Max(0, timeLimit - timeSpent);
            return remainingTime * 2;
        }

        private string GetAnswerMessage(bool isCorrect, int points, int streak)
        {
            if (!isCorrect)
                return "Resposta incorreta!";

            if (streak >= 5)
                return $"Incrível! Streak de {streak}! +{points} pontos";
            else if (streak >= 3)
                return $"Ótimo! Streak de {streak}! +{points} pontos";
            else
                return $"Correto! +{points} pontos";
        }

        private List<PowerUp> CheckPowerUps(PlayerData player, bool isCorrect, int pointsEarned)
        {
            var powerUps = new List<PowerUp>();

            if (!isCorrect)
                return powerUps;

            // Power-up por streak
            if (
                player.Streak == 5
                && !player.ActivePowerUps.Any(p => p.Type == PowerUpType.DoublePoints)
            )
            {
                powerUps.Add(
                    new PowerUp
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = "Pontos Duplos",
                        Description = "Ganhe o dobro de pontos por 3 perguntas",
                        Type = PowerUpType.DoublePoints,
                        Duration = 180, // 3 minutos
                        ExpiresAt = DateTime.UtcNow.AddMinutes(3),
                        IsActive = true,
                    }
                );
            }

            // Power-up por pontuação alta
            if (
                pointsEarned >= 150
                && !player.ActivePowerUps.Any(p => p.Type == PowerUpType.ExtraTime)
            )
            {
                powerUps.Add(
                    new PowerUp
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = "Tempo Extra",
                        Description = "Ganhe 15 segundos extras na próxima pergunta",
                        Type = PowerUpType.ExtraTime,
                        Duration = 15,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsActive = true,
                    }
                );
            }

            return powerUps;
        }

        private string GetOptionColor(int order)
        {
            return order switch
            {
                0 => "#FF6B6B", // Vermelho
                1 => "#4ECDC4", // Verde
                2 => "#45B7D1", // Azul
                3 => "#96CEB4", // Verde claro
                _ => "#DDA0DD", // Roxo
            };
        }

        private string GetOptionIcon(int order)
        {
            return order switch
            {
                0 => "🔴",
                1 => "🟢",
                2 => "🔵",
                3 => "🟡",
                _ => "🟣",
            };
        }

        #endregion
    }

    #region Internal Classes

    public class GameSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
        public int PesquisaId { get; set; }
        public string HostName { get; set; } = string.Empty;
        public GameStatus Status { get; set; }
        public GameSettings Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public Dictionary<string, PlayerData> Players { get; set; } = new();
        public List<GameQuestion> Questions { get; set; } = new();
    }

    public class PlayerData
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Score { get; set; }
        public int Streak { get; set; }
        public PlayerStatus Status { get; set; }
        public DateTime JoinedAt { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public List<int> ResponseTimes { get; set; } = new();
        public List<PowerUp> ActivePowerUps { get; set; } = new();
    }

    public class GameQuestion
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int Order { get; set; }
        public List<GameOption> Options { get; set; } = new();
        public DateTime? StartedAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public QuestionMedia? Media { get; set; }
        public List<GameAnswer> Answers { get; set; } = new();
    }

    public class GameOption
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class GameAnswer
    {
        public string PlayerId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public List<int> SelectedOptions { get; set; } = new();
        public string? TextAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public int TimeSpent { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    #endregion
}
