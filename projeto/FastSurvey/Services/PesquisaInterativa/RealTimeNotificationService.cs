using System.Text.Json;
using FASTSURVEY.Services.Result;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly ILogger<RealTimeNotificationService> _logger;
        private static readonly Dictionary<
            string,
            List<NotificationSubscription>
        > _sessionSubscriptions = new();
        private static readonly Dictionary<string, List<ChatMessage>> _sessionChats = new();
        private static readonly Dictionary<string, List<GameNotification>> _sessionNotifications =
            new();

        public RealTimeNotificationService(ILogger<RealTimeNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResult<bool>> SendGameUpdateAsync(
            string sessionId,
            GameUpdateType updateType,
            object data,
            CancellationToken ct = default
        )
        {
            try
            {
                var notification = new GameNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SessionId = sessionId,
                    Type = updateType,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    IsBroadcast = true,
                };

                // Armazenar notificação
                if (!_sessionNotifications.ContainsKey(sessionId))
                    _sessionNotifications[sessionId] = new List<GameNotification>();

                _sessionNotifications[sessionId].Add(notification);

                // Limpar notificações antigas (manter apenas as últimas 100)
                if (_sessionNotifications[sessionId].Count > 100)
                {
                    _sessionNotifications[sessionId] = _sessionNotifications[sessionId]
                        .OrderByDescending(n => n.Timestamp)
                        .Take(100)
                        .ToList();
                }

                _logger.LogInformation(
                    "Notificação enviada: {SessionId} - {UpdateType}",
                    sessionId,
                    updateType
                );

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao enviar notificação: {SessionId} - {UpdateType}",
                    sessionId,
                    updateType
                );
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar notificação: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> SendPlayerJoinedAsync(
            string sessionId,
            PlayerResponse player,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                player = new
                {
                    id = player.PlayerId,
                    name = player.PlayerName,
                    teamName = player.TeamName,
                    avatarUrl = player.AvatarUrl,
                    joinedAt = player.JoinedAt,
                },
                totalPlayers = GetActiveSubscriptions(sessionId).Count,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.PlayerJoined, data, ct);
        }

        public async Task<ServiceResult<bool>> SendPlayerLeftAsync(
            string sessionId,
            string playerId,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                playerId = playerId,
                totalPlayers = GetActiveSubscriptions(sessionId).Count - 1,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.PlayerLeft, data, ct);
        }

        public async Task<ServiceResult<bool>> SendQuestionStartedAsync(
            string sessionId,
            QuestionResponse question,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                question = new
                {
                    id = question.QuestionId,
                    text = question.Text,
                    type = question.Type,
                    timeLimit = question.TimeLimit,
                    options = question
                        .Options.Select(o => new
                        {
                            id = o.OptionId,
                            text = o.Text,
                            color = o.Color,
                            icon = o.Icon,
                        })
                        .ToList(),
                    startedAt = question.StartedAt,
                },
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.QuestionStarted, data, ct);
        }

        public async Task<ServiceResult<bool>> SendQuestionEndedAsync(
            string sessionId,
            int questionId,
            object results,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                questionId = questionId,
                results = results,
                endedAt = DateTime.UtcNow,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.QuestionEnded, data, ct);
        }

        public async Task<ServiceResult<bool>> SendAnswerReceivedAsync(
            string sessionId,
            string playerId,
            bool isCorrect,
            int points,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                playerId = playerId,
                isCorrect = isCorrect,
                points = points,
                timestamp = DateTime.UtcNow,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.AnswerReceived, data, ct);
        }

        public async Task<ServiceResult<bool>> SendLeaderboardUpdateAsync(
            string sessionId,
            LeaderboardResponse leaderboard,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                leaderboard = new
                {
                    players = leaderboard.Players.Take(10).ToList(), // Top 10
                    teams = leaderboard.Teams.Take(5).ToList(), // Top 5 teams
                    lastUpdated = leaderboard.LastUpdated,
                },
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.LeaderboardUpdate, data, ct);
        }

        public async Task<ServiceResult<bool>> SendGameEndedAsync(
            string sessionId,
            GameStatsResponse finalStats,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                finalStats = new
                {
                    totalPlayers = finalStats.TotalPlayers,
                    averageScore = finalStats.AverageScore,
                    highestScore = finalStats.HighestScore,
                    highestScorer = finalStats.HighestScorer,
                    sessionDuration = finalStats.SessionEnd - finalStats.SessionStart,
                },
                endedAt = DateTime.UtcNow,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.GameEnded, data, ct);
        }

        public async Task<ServiceResult<bool>> SendPowerUpActivatedAsync(
            string sessionId,
            string playerId,
            PowerUp powerUp,
            CancellationToken ct = default
        )
        {
            var data = new
            {
                playerId = playerId,
                powerUp = new
                {
                    id = powerUp.Id,
                    name = powerUp.Name,
                    description = powerUp.Description,
                    type = powerUp.Type,
                    duration = powerUp.Duration,
                    expiresAt = powerUp.ExpiresAt,
                },
                activatedAt = DateTime.UtcNow,
            };

            return await SendGameUpdateAsync(sessionId, GameUpdateType.PowerUpActivated, data, ct);
        }

        public async Task<ServiceResult<bool>> SendChatMessageAsync(
            string sessionId,
            ChatMessage message,
            CancellationToken ct = default
        )
        {
            try
            {
                // Armazenar mensagem no chat da sessão
                if (!_sessionChats.ContainsKey(sessionId))
                    _sessionChats[sessionId] = new List<ChatMessage>();

                _sessionChats[sessionId].Add(message);

                // Limpar mensagens antigas (manter apenas as últimas 200)
                if (_sessionChats[sessionId].Count > 200)
                {
                    _sessionChats[sessionId] = _sessionChats[sessionId]
                        .OrderByDescending(m => m.SentAt)
                        .Take(200)
                        .ToList();
                }

                var data = new
                {
                    message = new
                    {
                        id = message.Id,
                        playerId = message.PlayerId,
                        playerName = message.PlayerName,
                        message = message.Message,
                        type = message.Type,
                        sentAt = message.SentAt,
                        isSystemMessage = message.IsSystemMessage,
                    },
                };

                return await SendGameUpdateAsync(sessionId, GameUpdateType.ChatMessage, data, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem de chat: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar mensagem de chat: {ex.Message}"
                );
            }
        }

        public async Task<
            ServiceResult<List<NotificationSubscription>>
        > GetActiveSubscriptionsAsync(string sessionId, CancellationToken ct = default)
        {
            try
            {
                if (!_sessionSubscriptions.ContainsKey(sessionId))
                    return ServiceResult<List<NotificationSubscription>>.Ok(
                        new List<NotificationSubscription>()
                    );

                var activeSubscriptions = _sessionSubscriptions[sessionId]
                    .Where(s =>
                        s.LastActivity == null || s.LastActivity > DateTime.UtcNow.AddMinutes(-5)
                    )
                    .ToList();

                return ServiceResult<List<NotificationSubscription>>.Ok(activeSubscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter inscrições ativas: {SessionId}", sessionId);
                return ServiceResult<List<NotificationSubscription>>.Fail(
                    "ERROR",
                    $"Erro ao obter inscrições: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> SubscribeToSessionAsync(
            string sessionId,
            string connectionId,
            NotificationSubscription subscription,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_sessionSubscriptions.ContainsKey(sessionId))
                    _sessionSubscriptions[sessionId] = new List<NotificationSubscription>();

                // Remover inscrição existente se houver
                _sessionSubscriptions[sessionId].RemoveAll(s => s.ConnectionId == connectionId);

                // Adicionar nova inscrição
                subscription.ConnectionId = connectionId;
                subscription.SessionId = sessionId;
                subscription.SubscribedAt = DateTime.UtcNow;
                subscription.LastActivity = DateTime.UtcNow;

                _sessionSubscriptions[sessionId].Add(subscription);

                _logger.LogInformation(
                    "Inscrição criada: {SessionId} - {ConnectionId} - {PlayerName}",
                    sessionId,
                    connectionId,
                    subscription.PlayerName
                );

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao inscrever na sessão: {SessionId} - {ConnectionId}",
                    sessionId,
                    connectionId
                );
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao inscrever: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UnsubscribeFromSessionAsync(
            string sessionId,
            string connectionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (_sessionSubscriptions.ContainsKey(sessionId))
                {
                    _sessionSubscriptions[sessionId].RemoveAll(s => s.ConnectionId == connectionId);
                }

                _logger.LogInformation(
                    "Inscrição removida: {SessionId} - {ConnectionId}",
                    sessionId,
                    connectionId
                );

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao cancelar inscrição: {SessionId} - {ConnectionId}",
                    sessionId,
                    connectionId
                );
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao cancelar inscrição: {ex.Message}"
                );
            }
        }

        #region Private Methods

        private List<NotificationSubscription> GetActiveSubscriptions(string sessionId)
        {
            if (!_sessionSubscriptions.ContainsKey(sessionId))
                return new List<NotificationSubscription>();

            return _sessionSubscriptions[sessionId]
                .Where(s =>
                    s.LastActivity == null || s.LastActivity > DateTime.UtcNow.AddMinutes(-5)
                )
                .ToList();
        }

        public async Task<ServiceResult<bool>> UpdatePlayerActivityAsync(
            string sessionId,
            string connectionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (_sessionSubscriptions.ContainsKey(sessionId))
                {
                    var subscription = _sessionSubscriptions[sessionId]
                        .FirstOrDefault(s => s.ConnectionId == connectionId);

                    if (subscription != null)
                    {
                        subscription.LastActivity = DateTime.UtcNow;
                    }
                }

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao atualizar atividade do jogador: {SessionId} - {ConnectionId}",
                    sessionId,
                    connectionId
                );
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao atualizar atividade: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<List<ChatMessage>>> GetSessionChatAsync(
            string sessionId,
            int limit = 50,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_sessionChats.ContainsKey(sessionId))
                    return ServiceResult<List<ChatMessage>>.Ok(new List<ChatMessage>());

                var messages = _sessionChats[sessionId]
                    .OrderByDescending(m => m.SentAt)
                    .Take(limit)
                    .OrderBy(m => m.SentAt)
                    .ToList();

                return ServiceResult<List<ChatMessage>>.Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter chat da sessão: {SessionId}", sessionId);
                return ServiceResult<List<ChatMessage>>.Fail(
                    "ERROR",
                    $"Erro ao obter chat: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<List<GameNotification>>> GetSessionNotificationsAsync(
            string sessionId,
            int limit = 20,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_sessionNotifications.ContainsKey(sessionId))
                    return ServiceResult<List<GameNotification>>.Ok(new List<GameNotification>());

                var notifications = _sessionNotifications[sessionId]
                    .OrderByDescending(n => n.Timestamp)
                    .Take(limit)
                    .OrderBy(n => n.Timestamp)
                    .ToList();

                return ServiceResult<List<GameNotification>>.Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao obter notificações da sessão: {SessionId}",
                    sessionId
                );
                return ServiceResult<List<GameNotification>>.Fail(
                    "ERROR",
                    $"Erro ao obter notificações: {ex.Message}"
                );
            }
        }

        #endregion
    }
}
