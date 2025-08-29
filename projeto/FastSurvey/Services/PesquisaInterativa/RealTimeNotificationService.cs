#nullable enable
using FASTSURVEY.Services.Result;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly ILogger<RealTimeNotificationService> _logger;
        private static readonly Dictionary<string, List<NotificationSubscription>> _sessionSubscriptions = new();
        private static readonly Dictionary<string, List<SessionNotification>> _sessionNotifications = new();

        public RealTimeNotificationService(ILogger<RealTimeNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResult<bool>> SendQuestionUpdateAsync(
            string sessionId,
            QuestionUpdate update,
            CancellationToken ct = default
        )
        {
            try
            {
                var notification = new SessionNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SessionId = sessionId,
                    Type = SessionUpdateType.QuestionChanged,
                    Data = update,
                    Timestamp = DateTime.UtcNow,
                    IsBroadcast = true,
                };

                await StoreNotificationAsync(sessionId, notification);
                await BroadcastToSessionAsync(sessionId, notification, ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar atualização de pergunta: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar atualização de pergunta: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> SendParticipantUpdateAsync(
            string sessionId,
            ParticipantUpdate update,
            CancellationToken ct = default
        )
        {
            try
            {
                var notification = new SessionNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SessionId = sessionId,
                    Type = update.Tipo,
                    Data = update,
                    Timestamp = DateTime.UtcNow,
                    IsBroadcast = true,
                };

                await StoreNotificationAsync(sessionId, notification);
                await BroadcastToSessionAsync(sessionId, notification, ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar atualização de participante: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar atualização de participante: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> SendSessionStatusUpdateAsync(
            string sessionId,
            SessionStatusUpdate update,
            CancellationToken ct = default
        )
        {
            try
            {
                var notification = new SessionNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SessionId = sessionId,
                    Type = SessionUpdateType.SessionStatusChanged,
                    Data = update,
                    Timestamp = DateTime.UtcNow,
                    IsBroadcast = true,
                };

                await StoreNotificationAsync(sessionId, notification);
                await BroadcastToSessionAsync(sessionId, notification, ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar atualização de status da sessão: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar atualização de status da sessão: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> SendAnswerReceivedAsync(
            string sessionId,
            AnswerReceived update,
            CancellationToken ct = default
        )
        {
            try
            {
                var notification = new SessionNotification
                {
                    Id = Guid.NewGuid().ToString("N"),
                    SessionId = sessionId,
                    Type = SessionUpdateType.AnswerReceived,
                    Data = update,
                    Timestamp = DateTime.UtcNow,
                    IsBroadcast = true,
                };

                await StoreNotificationAsync(sessionId, notification);
                await BroadcastToSessionAsync(sessionId, notification, ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de resposta recebida: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao enviar notificação de resposta recebida: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<List<NotificationSubscription>>> GetActiveSubscriptionsAsync(
            string sessionId,
            CancellationToken ct = default
        )
        {
            try
            {
                if (!_sessionSubscriptions.ContainsKey(sessionId))
                    return ServiceResult<List<NotificationSubscription>>.Ok(new List<NotificationSubscription>());

                var subscriptions = _sessionSubscriptions[sessionId]
                    .Where(s => s.LastActivity == null || s.LastActivity > DateTime.UtcNow.AddMinutes(-5))
                    .ToList();

                return ServiceResult<List<NotificationSubscription>>.Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter inscrições ativas: {SessionId}", sessionId);
                return ServiceResult<List<NotificationSubscription>>.Fail(
                    "ERROR",
                    $"Erro ao obter inscrições ativas: {ex.Message}"
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

                _logger.LogInformation("Participante inscrito na sessão: {SessionId}, ConnectionId: {ConnectionId}", sessionId, connectionId);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inscrever na sessão: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao inscrever na sessão: {ex.Message}"
                );
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

                _logger.LogInformation("Participante desinscrito da sessão: {SessionId}, ConnectionId: {ConnectionId}", sessionId, connectionId);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desinscrever da sessão: {SessionId}", sessionId);
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao desinscrever da sessão: {ex.Message}"
                );
            }
        }

        // Métodos auxiliares privados
        private async Task StoreNotificationAsync(string sessionId, SessionNotification notification)
        {
            if (!_sessionNotifications.ContainsKey(sessionId))
                _sessionNotifications[sessionId] = new List<SessionNotification>();

            _sessionNotifications[sessionId].Add(notification);

            // Limpar notificações antigas (manter apenas as últimas 100)
            if (_sessionNotifications[sessionId].Count > 100)
            {
                _sessionNotifications[sessionId] = _sessionNotifications[sessionId]
                    .OrderByDescending(n => n.Timestamp)
                    .Take(100)
                    .ToList();
            }
        }

        private async Task BroadcastToSessionAsync(string sessionId, SessionNotification notification, CancellationToken ct)
        {
            if (!_sessionSubscriptions.ContainsKey(sessionId))
                return;

            var subscriptions = _sessionSubscriptions[sessionId]
                .Where(s => s.SubscribedEvents.Contains(notification.Type))
                .ToList();

            foreach (var subscription in subscriptions)
            {
                try
                {
                    // Aqui você implementaria a lógica real de envio de notificação
                    // Por exemplo, usando SignalR, WebSockets, ou outro mecanismo
                    _logger.LogDebug("Enviando notificação para: {ConnectionId}, Tipo: {Type}", 
                        subscription.ConnectionId, notification.Type);

                    // Atualizar última atividade
                    subscription.LastActivity = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar notificação para: {ConnectionId}", subscription.ConnectionId);
                }
            }
        }

        // Método para limpeza periódica de inscrições inativas
        public static void CleanupInactiveSubscriptions()
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
            
            foreach (var sessionId in _sessionSubscriptions.Keys.ToList())
            {
                _sessionSubscriptions[sessionId] = _sessionSubscriptions[sessionId]
                    .Where(s => s.LastActivity > cutoffTime)
                    .ToList();

                if (_sessionSubscriptions[sessionId].Count == 0)
                    _sessionSubscriptions.Remove(sessionId);
            }
        }
    }
}
