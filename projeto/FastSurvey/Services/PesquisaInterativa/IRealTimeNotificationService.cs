using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public interface IRealTimeNotificationService
    {
        Task<ServiceResult<bool>> SendGameUpdateAsync(
            string sessionId,
            GameUpdateType updateType,
            object data,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendPlayerJoinedAsync(
            string sessionId,
            PlayerResponse player,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendPlayerLeftAsync(
            string sessionId,
            string playerId,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendQuestionStartedAsync(
            string sessionId,
            QuestionResponse question,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendQuestionEndedAsync(
            string sessionId,
            int questionId,
            object results,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendAnswerReceivedAsync(
            string sessionId,
            string playerId,
            bool isCorrect,
            int points,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendLeaderboardUpdateAsync(
            string sessionId,
            LeaderboardResponse leaderboard,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendGameEndedAsync(
            string sessionId,
            GameStatsResponse finalStats,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendPowerUpActivatedAsync(
            string sessionId,
            string playerId,
            PowerUp powerUp,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> SendChatMessageAsync(
            string sessionId,
            ChatMessage message,
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

    public enum GameUpdateType
    {
        PlayerJoined = 1,
        PlayerLeft = 2,
        QuestionStarted = 3,
        QuestionEnded = 4,
        AnswerReceived = 5,
        LeaderboardUpdate = 6,
        GameEnded = 7,
        PowerUpActivated = 8,
        ChatMessage = 9,
        TimerUpdate = 10,
        SettingsChanged = 11,
    }

    public class NotificationSubscription
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public bool IsHost { get; set; }
        public List<GameUpdateType> SubscribedEvents { get; set; } = new();
        public DateTime SubscribedAt { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class ChatMessage
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSystemMessage { get; set; }
    }

    public enum MessageType
    {
        Text = 1,
        Emoji = 2,
        System = 3,
        PowerUp = 4,
        Achievement = 5,
    }

    public class GameNotification
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public GameUpdateType Type { get; set; }
        public object Data { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string? TargetPlayerId { get; set; }
        public bool IsBroadcast { get; set; }
    }

    public class NotificationSettings
    {
        public bool EnableSound { get; set; } = true;
        public bool EnableVibration { get; set; } = true;
        public bool EnableDesktopNotifications { get; set; } = true;
        public bool EnableChat { get; set; } = true;
        public bool EnableLeaderboardUpdates { get; set; } = true;
        public bool EnablePowerUpNotifications { get; set; } = true;
        public int NotificationTimeout { get; set; } = 5000; // ms
    }
}
