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
    }

    // ---------- SESSÃO DE RESPOSTA ----------
    public class IniciarRespostaRequest
    {
        public int PesquisaId { get; set; }
        public InformacoesRespondenteRequest? Respondente { get; set; }
        public string? CodigoAcesso { get; set; } // Para pesquisas interativas
        public bool RespostaAnonima { get; set; } = false;
    }

    public class CriarRespostaDiscursivaRequest
    {
        [Required] public int PerguntaId { get; set; }
        [Required, MinLength(1)] public string Texto { get; set; } = string.Empty;

        // metadados opcionais (não persistidos aqui)
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
        public string? SessaoId { get; set; } // Para pesquisas interativas
        public bool RespostaAnonima { get; set; } = false;
    }

    public class CriarRespostaOpcoesRequest
    {
        [Required] public int PerguntaId { get; set; }
        [Required] public List<int> OpcoesSelecionadas { get; set; } = new();

        // metadados opcionais (não persistidos aqui)
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
        public string? SessaoId { get; set; } // Para pesquisas interativas
        public bool RespostaAnonima { get; set; } = false;
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
    }
}
