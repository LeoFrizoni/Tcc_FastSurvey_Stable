// ---------- CREATE ----------
using System.ComponentModel.DataAnnotations;

// ---------- CREATE ----------
public class CriarPesquisaRequest
{
    [Required] public int LoginId { get; set; }
    [Required] public int TipoPesquisaId { get; set; }
    public int? PastaId { get; set; }

    [Required, StringLength(160)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Descricao { get; set; }

    // JSON do construtor (array de blocos). Salve como string.
    public string? TemplateJson { get; set; }

    // Opcional: URL do QR code, se já existir.
    public string? QrCodeUrl { get; set; }

    // NOVOS CAMPOS NECESSÁRIOS
    public bool TemLimitadorTempo { get; set; } = false;
    public DateTime? DataFechamento { get; set; }
    public bool IsInterativa { get; set; } = false; // Para funcionalidade tipo Kahoot
    public bool PermiteRespostasAnonimas { get; set; } = true;
    public int? LimiteRespostas { get; set; } // Para usuários premium
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

    // Se quiser trocar o QR armazenado
    public string? QrCodeUrl { get; set; }

    // NOVOS CAMPOS
    public bool? TemLimitadorTempo { get; set; }
    public DateTime? DataFechamento { get; set; }
    public bool? IsInterativa { get; set; }
    public bool? PermiteRespostasAnonimas { get; set; }
    public int? LimiteRespostas { get; set; }
}

// ---------- PATCH TEMPLATE ----------
public class AtualizarTemplateRequest
{
    [Required] public string TemplateJson { get; set; } = "[]";
}

// ---------- FILTRO LISTAGEM ----------
public class PesquisaFiltroRequest
{
    public int? LoginId { get; set; }
    public int? PastaId { get; set; }
    public int? TipoPesquisaId { get; set; }
    public string? Busca { get; set; } // procura em título/descrição
    public int? Page { get; set; } = 1;       // 1-based
    public int? PageSize { get; set; } = 20;  // padrão
    public bool? IsInterativa { get; set; }   // Filtrar por tipo
    public bool? Ativa { get; set; }          // Filtrar por status
}

// ---------- NOVOS DTOs PARA FUNCIONALIDADES ESPECÍFICAS ----------
public class PesquisaInterativaRequest
{
    public int PesquisaId { get; set; }
    public bool IsAtiva { get; set; }
    public string? CodigoAcesso { get; set; } // Para participantes entrarem
}

public class ResultadoPesquisaRequest
{
    public int PesquisaId { get; set; }
    public string Formato { get; set; } = "json"; // json, pdf, excel
    public bool IncluirGraficos { get; set; } = true;
}

public class QRCodeRequest
{
    public int PesquisaId { get; set; }
    public bool GerarNovo { get; set; } = false;
    public DateTime? Expiracao { get; set; }
}