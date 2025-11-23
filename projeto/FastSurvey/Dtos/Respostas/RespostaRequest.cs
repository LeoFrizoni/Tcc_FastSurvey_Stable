#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Respostas
{
    // ---------- INFORMAÇÕES DO RESPONDENTE ----------
    public class InformacoesRespondenteRequest
    {
        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Empresa { get; set; }

        [StringLength(50)]
        public string? Cargo { get; set; }
        public bool AceitaTermos { get; set; } = false;

        // Novos campos
        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }
        public DateTime? DataNascimento { get; set; }

        [StringLength(50)]
        public string? Genero { get; set; }
    }

    // ---------- SESSÃO DE RESPOSTA ----------
    public class IniciarRespostaRequest
    {
        public int PesquisaId { get; set; }
        public InformacoesRespondenteRequest? Respondente { get; set; }
        public string? CodigoAcesso { get; set; }
        public bool RespostaAnonima { get; set; } = false;

        // Analytics
        public string? Origem { get; set; } // "qr", "link", "interativo"
        public string? UserAgent { get; set; }
        public string? IPAddress { get; set; }
    }

    // ---------- CRIAÇÃO DE RESPOSTAS ----------
    public class CriarRespostaDiscursivaRequest
    {
        [Required]
        public int PerguntaId { get; set; }

        [Required, MinLength(1)]
        public string Texto { get; set; } = string.Empty;

        // Metadados opcionais
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
        public string? SessaoId { get; set; }
        public bool RespostaAnonima { get; set; } = false;
        public int? ParticipanteId { get; set; }
    }

    public class CriarRespostaOpcoesRequest
    {
        [Required]
        public int PerguntaId { get; set; }

        [Required]
        public List<int> OpcoesSelecionadas { get; set; } = new();

        // Metadados opcionais
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
        public string? SessaoId { get; set; }
        public bool RespostaAnonima { get; set; } = false;
        public int? ParticipanteId { get; set; }
    }

    public class RespostaDto
    {
        public int RespostaId { get; set; }
        public int PerguntaId { get; set; }
        public string? Texto { get; set; }
        public DateTime DataResposta { get; set; }
        public List<int> Opcoes { get; set; } = new();
        public bool RespostaAnonima { get; set; }
        public string? SessaoId { get; set; }
        public int? ParticipanteId { get; set; }
    }

    // ---------- RESULTADOS E ESTATÍSTICAS ----------
    public class EstatisticasPerguntaRequest
    {
        public int PerguntaId { get; set; }
        public bool IncluirGraficos { get; set; } = true;
        public string? TipoGrafico { get; set; } // "pie", "bar", "line"
    }

    public class EstatisticasPesquisaRequest
    {
        public int PesquisaId { get; set; }
        public bool IncluirGraficos { get; set; } = true;
        public bool IncluirRespostasDetalhadas { get; set; } = false;
        public string Formato { get; set; } = "json"; // json, pdf, excel

        // Filtros
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? FiltroOrigem { get; set; }
        public bool IncluirAnalytics { get; set; } = false;
    }

    // ---------- ENTRADA COMPLETA ----------
    public class EnviarRespostaCompletaRequest
    {
        public int PesquisaId { get; set; }
        public InformacoesRespondenteRequest? Respondente { get; set; }
        public bool RespostaAnonima { get; set; } = false;
        public List<RespostaItem> Respostas { get; set; } = new();
        public string? SessaoId { get; set; }
        public string? Origem { get; set; }
        public int? ParticipanteId { get; set; }
    }

    public class RespostaItem
    {
        public int PerguntaId { get; set; }
        public string? Texto { get; set; }
        public List<int>? OpcoesSelecionadas { get; set; }
        public string? OutraOpcao { get; set; }
        public List<string>? Anexos { get; set; }
        public int? TempoResposta { get; set; }
    }

    public class ValidarRespostaRequest
    {
        public int PesquisaId { get; set; }
        public List<RespostaItem> Respostas { get; set; } = new();
        public bool RespostaAnonima { get; set; } = false;
    }
}
