// ---------- CREATE ----------
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pesquisas
{
    // ---------- CREATE ----------
    public class CriarPesquisaRequest
    {
        [Required]
        public int LoginId { get; set; }

        [Required]
        public int TipoPesquisaId { get; set; }
        public int? PastaId { get; set; }

        [Required, StringLength(160)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descricao { get; set; }

        public string? TemplateJson { get; set; }
        public string? QRCodeUrl { get; set; }

        public bool TemLimitadorTempo { get; set; } = false;
        public DateTime? DataFechamento { get; set; }
        public bool IsInterativa { get; set; } = false;
        public bool PermiteRespostasAnonimas { get; set; } = true;
        public int? LimiteRespostas { get; set; }
        public bool Ativa { get; set; } = true;

        // Só mantenha estes se a Model/DB já tiver colunas
        public string? Slug { get; set; }
        public bool RequerIdentificacao { get; set; } = false;
        public string? Instrucoes { get; set; }
        public bool MostrarProgresso { get; set; } = true;
        public bool PermitirEdicao { get; set; } = false;
        public int? TempoLimitePorPergunta { get; set; }
    }

    // ---------- UPDATE (PUT) ----------
    public class AtualizarPesquisaRequest
    {
        [Required, StringLength(160)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descricao { get; set; }

        public int? PastaId { get; set; }
        public int? TipoPesquisaId { get; set; }
        public string? QRCodeUrl { get; set; }

        public bool? TemLimitadorTempo { get; set; }
        public DateTime? DataFechamento { get; set; }
        public bool? IsInterativa { get; set; }
        public bool? PermiteRespostasAnonimas { get; set; }
        public int? LimiteRespostas { get; set; }
        public bool? Ativa { get; set; }

        // idem observação
        public string? Slug { get; set; }
        public bool? RequerIdentificacao { get; set; }
        public string? Instrucoes { get; set; }
        public bool? MostrarProgresso { get; set; }
        public bool? PermitirEdicao { get; set; }
        public int? TempoLimitePorPergunta { get; set; }
    }

    // ---------- PATCH TEMPLATE ----------
    public class AtualizarTemplateRequest
    {
        [Required]
        public string TemplateJson { get; set; } = "[]";
    }

    // ---------- FILTRO LISTAGEM ----------
    public class PesquisaFiltroRequest
    {
        public int? LoginId { get; set; }
        public int? PastaId { get; set; }
        public int? TipoPesquisaId { get; set; }
        public string? Busca { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;

        public bool? IsInterativa { get; set; }
        public bool? Ativa { get; set; }
        public bool? TemLimitadorTempo { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    // ---------- ESPECIAIS ----------
    public class DuplicarPesquisaRequest
    {
        public int PesquisaId { get; set; }
        public string? NovoTitulo { get; set; }
        public int? NovaPastaId { get; set; }
        public bool IncluirRespostas { get; set; } = false;
    }

    public class ValidarPesquisaRequest
    {
        public int PesquisaId { get; set; }
        public string? CodigoAcesso { get; set; }
        public bool RespostaAnonima { get; set; } = false;
    }
}
