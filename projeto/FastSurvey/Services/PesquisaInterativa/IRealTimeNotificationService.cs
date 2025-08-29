#nullable enable
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public interface IRealTimeNotificationService
    {
        Task<ServiceResult<bool>> SendQuestionUpdateAsync(
            string sessionId,
            QuestionUpdate update,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendParticipantUpdateAsync(
            string sessionId,
            ParticipantUpdate update,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendSessionStatusUpdateAsync(
            string sessionId,
            SessionStatusUpdate update,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendAnswerReceivedAsync(
            string sessionId,
            AnswerReceived update,
            CancellationToken ct = default
        );
        Task<ServiceResult<List<NotificationSubscription>>> GetActiveSubscriptionsAsync(
            string sessionId,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SubscribeToSessionAsync(
            string sessionId,
            string connectionId,
            NotificationSubscription subscription,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> UnsubscribeFromSessionAsync(
            string sessionId,
            string connectionId,
            CancellationToken ct = default
        );
    }

    public enum SessionUpdateType
    {
        ParticipantJoined = 1,
        ParticipantLeft = 2,
        QuestionChanged = 3,
        QuestionActivated = 4,
        QuestionDeactivated = 5,
        AnswerReceived = 6,
        SessionEnded = 7,
        SessionStatusChanged = 8,
    }

    public class NotificationSubscription
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string? ParticipantId { get; set; }
        public string? ParticipantName { get; set; }
        public bool IsHost { get; set; }
        public List<SessionUpdateType> SubscribedEvents { get; set; } = new();
        public DateTime SubscribedAt { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class QuestionUpdate
    {
        public int? PerguntaId { get; set; }
        public int? Ordem { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public bool Ativa { get; set; }
        public List<object> Opcoes { get; set; } = new();
        public int TotalPerguntas { get; set; }
        public bool TemProxima { get; set; }
        public bool TemAnterior { get; set; }
    }

    public class ParticipantUpdate
    {
        public int ParticipanteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime EntrouEm { get; set; }
        public SessionUpdateType Tipo { get; set; }
    }

    public class SessionStatusUpdate
    {
        public string SessaoId { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public int TotalParticipantes { get; set; }
        public int? PerguntaAtualId { get; set; }
        public bool PerguntaAtiva { get; set; }
        public bool ModoApresentacao { get; set; }
    }

    public class AnswerReceived
    {
        public int ParticipanteId { get; set; }
        public string NomeParticipante { get; set; } = string.Empty;
        public int PerguntaId { get; set; }
        public DateTime RespondidaEm { get; set; }
    }

    public class SessionNotification
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public SessionUpdateType Type { get; set; }
        public object Data { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string? TargetParticipantId { get; set; }
        public bool IsBroadcast { get; set; }
    }

    public class NotificationSettings
    {
        public bool EnableSound { get; set; } = true;
        public bool EnableVibration { get; set; } = true;
        public bool EnableDesktopNotifications { get; set; } = true;
        public bool EnableQuestionUpdates { get; set; } = true;
        public bool EnableParticipantUpdates { get; set; } = true;
        public int NotificationTimeout { get; set; } = 5000; // ms
    }
}
