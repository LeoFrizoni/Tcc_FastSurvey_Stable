using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public interface IGamificationService
    {
        Task<ServiceResult<GameSessionResponse>> CreateGameSessionAsync(
            CreateGameSessionRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<PlayerResponse>> JoinGameAsync(
            JoinGameRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<QuestionResponse>> GetCurrentQuestionAsync(
            string sessionId,
            CancellationToken ct = default
        );
        Task<ServiceResult<AnswerResponse>> SubmitAnswerAsync(
            SubmitAnswerRequest request,
            CancellationToken ct = default
        );
        Task<ServiceResult<LeaderboardResponse>> GetLeaderboardAsync(
            string sessionId,
            CancellationToken ct = default
        );
        Task<ServiceResult<GameStatsResponse>> GetGameStatsAsync(
            string sessionId,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> StartQuestionTimerAsync(
            string sessionId,
            int questionId,
            int timeLimit,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> EndQuestionAsync(
            string sessionId,
            int questionId,
            CancellationToken ct = default
        );
        Task<ServiceResult<GameSettingsResponse>> UpdateGameSettingsAsync(
            string sessionId,
            GameSettingsRequest request,
            CancellationToken ct = default
        );
    }

    public class CreateGameSessionRequest
    {
        public int PesquisaId { get; set; }
        public string HostName { get; set; } = string.Empty;
        public GameSettings Settings { get; set; } = new();
    }

    public class GameSettings
    {
        public bool EnableTimer { get; set; } = true;
        public int DefaultTimeLimit { get; set; } = 30; // segundos
        public bool ShowLeaderboard { get; set; } = true;
        public bool EnableSound { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public bool AllowLateJoining { get; set; } = true;
        public int MaxPlayers { get; set; } = 100;
        public bool EnableTeamMode { get; set; } = false;
        public bool ShowCorrectAnswers { get; set; } = true;
        public bool EnablePowerUps { get; set; } = false;
    }

    public class GameSessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public GameStatus Status { get; set; }
        public GameSettings Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int TotalPlayers { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public string QrCodeUrl { get; set; } = string.Empty;
    }

    public class JoinGameRequest
    {
        public string AccessCode { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class PlayerResponse
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Score { get; set; }
        public int Streak { get; set; }
        public PlayerStatus Status { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<PowerUp> ActivePowerUps { get; set; } = new();
    }

    public class QuestionResponse
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int TimeLimit { get; set; }
        public int Order { get; set; }
        public List<OptionResponse> Options { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public QuestionMedia? Media { get; set; }
    }

    public class OptionResponse
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }

    public class QuestionMedia
    {
        public string Type { get; set; } = string.Empty; // image, video, audio
        public string Url { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }

    public class SubmitAnswerRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public List<int> SelectedOptions { get; set; } = new();
        public string? TextAnswer { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int TimeSpent { get; set; } // segundos
    }

    public class AnswerResponse
    {
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public int StreakBonus { get; set; }
        public int TimeBonus { get; set; }
        public int TotalPoints { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PowerUp> EarnedPowerUps { get; set; } = new();
    }

    public class LeaderboardResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public List<LeaderboardEntry> Players { get; set; } = new();
        public List<LeaderboardEntry> Teams { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class LeaderboardEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Position { get; set; }
        public int Streak { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public double Accuracy { get; set; }
        public string? AvatarUrl { get; set; }
        public string? TeamName { get; set; }
    }

    public class GameStatsResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public int TotalPlayers { get; set; }
        public int ActivePlayers { get; set; }
        public int TotalQuestions { get; set; }
        public int CompletedQuestions { get; set; }
        public double AverageScore { get; set; }
        public int HighestScore { get; set; }
        public string? HighestScorer { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public Dictionary<string, int> QuestionStats { get; set; } = new();
        public DateTime SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
    }

    public class GameSettingsRequest
    {
        public bool? EnableTimer { get; set; }
        public int? DefaultTimeLimit { get; set; }
        public bool? ShowLeaderboard { get; set; }
        public bool? EnableSound { get; set; }
        public bool? EnableAnimations { get; set; }
        public bool? AllowLateJoining { get; set; }
        public int? MaxPlayers { get; set; }
        public bool? EnableTeamMode { get; set; }
        public bool? ShowCorrectAnswers { get; set; }
        public bool? EnablePowerUps { get; set; }
    }

    public class GameSettingsResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public GameSettings Settings { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public enum GameStatus
    {
        Waiting = 0,
        Starting = 1,
        Active = 2,
        Paused = 3,
        Finished = 4,
        Cancelled = 5,
    }

    public enum PlayerStatus
    {
        Connected = 0,
        Disconnected = 1,
        Spectator = 2,
        Eliminated = 3,
    }

    public enum QuestionType
    {
        MultipleChoice = 1,
        TrueFalse = 2,
        Text = 3,
        DragAndDrop = 4,
        Ordering = 5,
        Matching = 6,
    }

    public class PowerUp
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PowerUpType Type { get; set; }
        public int Duration { get; set; } // segundos
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }

    public enum PowerUpType
    {
        DoublePoints = 1,
        ExtraTime = 2,
        SkipQuestion = 3,
        EliminateOption = 4,
        StreakProtection = 5,
        TeamBonus = 6,
    }
}
