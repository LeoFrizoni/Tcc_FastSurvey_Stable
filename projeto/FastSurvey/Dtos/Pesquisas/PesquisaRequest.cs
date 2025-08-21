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
}