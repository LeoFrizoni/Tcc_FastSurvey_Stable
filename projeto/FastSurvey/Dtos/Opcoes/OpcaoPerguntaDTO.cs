using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Opcoes
{
    // ---------- CREATE / UPSERT ----------
    public class OpcaoPerguntaRequest
    {
        // Se a opção for criada já vinculada à pergunta no Service,
        // você pode omitir PerguntaId no body e setar pelo caminho/rota.
        public int? PerguntaId { get; set; }

        [Required, StringLength(160)]
        public string Texto { get; set; } = "";

        // Define a ordem da opção dentro da pergunta
        public int Ordem { get; set; } = 0;

        // Controle de exibição sem precisar excluir fisicamente
        public bool Ativa { get; set; } = true;

        // Marcador de gabarito (quando aplicável)
        public bool? Correta { get; set; }
    }

    // ---------- UPDATE ----------
    public class OpcaoPerguntaUpdateRequest
    {
        [Required]
        public int OpcaoId { get; set; }

        // Atualize apenas o que precisar
        [StringLength(160)]
        public string? Texto { get; set; }

        public int? Ordem { get; set; }

        public bool? Ativa { get; set; }

        public bool? Correta { get; set; }
    }

    // ---------- RESPONSE ----------
    public class OpcaoPerguntaResponse
    {
        public int OpcaoId { get; set; }
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = "";
        public int Ordem { get; set; }
        public bool Ativa { get; set; }
        public bool Correta { get; set; }
    }
}
