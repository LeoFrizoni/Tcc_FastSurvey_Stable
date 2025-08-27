#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pesquisas
{
    /// <summary>
    /// Iniciar uma sessão interativa para uma pesquisa.
    /// </summary>
    public sealed class PesquisaInterativaRequest
    {
        [Required]
        public int PesquisaId { get; set; }

        /// <summary>
        /// Se true, a sessão já começa marcada como iniciada (IniciadaEm = agora).
        /// </summary>
        public bool IsAtiva { get; set; } = true;
    }

    /// <summary>
    /// Entrar em uma sessão existente usando o código de acesso.
    /// </summary>
    public sealed class EntrarSessaoRequest
    {
        [Required, MinLength(4)]
        public string CodigoAcesso { get; set; } = string.Empty;

        /// <summary>
        /// Opcional; se vazio, será salvo como "Participante".
        /// </summary>
        public string? NomeParticipante { get; set; }
    }

    /// <summary>
    /// Enviar resposta para uma pergunta durante a sessão.
    /// </summary>
    public sealed class RespostaInterativaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;

        [Required]
        public int ParticipanteId { get; set; }

        [Required]
        public int PerguntaId { get; set; }

        /// <summary>
        /// Para perguntas discursivas (Texto obrigatório).
        /// </summary>
        public string? TextoResposta { get; set; }

        /// <summary>
        /// Para objetiva/múltipla: ids das opções selecionadas.
        /// </summary>
        public List<int> OpcoesSelecionadas { get; set; } = new();
    }

    /// <summary>
    /// Finalizar (encerrar) uma sessão em andamento.
    /// </summary>
    public sealed class FinalizarSessaoRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
    }
}
