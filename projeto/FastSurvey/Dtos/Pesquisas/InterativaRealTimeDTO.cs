#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pesquisas
{
    // Representa um jogador conectado à sessão (usado em PlayerJoined/Left, leaderboard etc.)
    public sealed class PlayerResponse
    {
        [Required]
        public string PlayerId { get; set; } = string.Empty;

        [Required]
        public string PlayerName { get; set; } = string.Empty;

        public string? TeamName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    // Opção de pergunta (usada dentro de QuestionResponse)
    public sealed class OptionResponse
    {
        [Required]
        public int OptionId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        // Campos visuais opcionais, usados pelo serviço
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int? Order { get; set; }
    }

    // Pergunta ativa/iniciada (QuestionStarted/Ended)
    public sealed class QuestionResponse
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da pergunta (ex.: 1=Discursiva, 2=Objetiva, 3=Múltipla)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Tempo limite em segundos (se aplicável).
        /// </summary>
        public int? TimeLimit { get; set; }

        public List<OptionResponse> Options { get; set; } = new();

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    // Resumo de pontuação por jogador (aparece no leaderboard)
    public sealed class PlayerScore
    {
        [Required]
        public string PlayerId { get; set; } = string.Empty;

        [Required]
        public string PlayerName { get; set; } = string.Empty;

        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public string? TeamName { get; set; }
        public string? AvatarUrl { get; set; }
    }

    // Resumo de pontuação por equipe
    public sealed class TeamScore
    {
        [Required]
        public string TeamName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Members { get; set; }
    }

    // Payload de leaderboard (Top jogadores e equipes)
    public sealed class LeaderboardResponse
    {
        public List<PlayerScore> Players { get; set; } = new();
        public List<TeamScore> Teams { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Estatísticas finais da sessão (GameEnded)
    public sealed class GameStatsResponse
    {
        public int TotalPlayers { get; set; }
        public double AverageScore { get; set; }
        public int HighestScore { get; set; }
        public string? HighestScorer { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
    }

    public enum PowerUpType
    {
        SpeedBoost = 1,
        DoublePoints = 2,
        FreezeOthers = 3,
        RevealHint = 4,
        Shield = 5,
    }

    // Power-up ativado por um jogador
    public sealed class PowerUp
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PowerUpType Type { get; set; } = PowerUpType.DoublePoints;

        /// <summary>Duração (segundos) caso aplicável.</summary>
        public int? Duration { get; set; }

        /// <summary>Expiração absoluta caso aplicável.</summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
