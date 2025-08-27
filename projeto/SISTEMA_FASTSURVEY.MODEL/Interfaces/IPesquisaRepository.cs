#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
public interface IPesquisaRepository : IRepository<Pesquisas>
{
    Task<Pesquisas?> GetDetalheAsync(
        int pesquisaId,
        bool incluirPerguntas = true,
        bool incluirAnexos = false,
        bool incluirTipoPesquisa = false,
        bool incluirLogin = false,
        bool incluirPasta = false,
        bool incluirSessoes = false,
        CancellationToken ct = default
    );

    Task<List<Pesquisas>> ListarPorLoginAsync(
        int loginId,
        int? pastaId = null,
        int? tipoPesquisaId = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default
    );

    /// <summary>
    /// Projeção leve para cards da Home/Lista (sem rastrear).
    /// </summary>
    Task<List<PesquisaResumoDto>> ListarResumoPorLoginAsync(
        int loginId,
        int? pastaId = null,
        int? tipoPesquisaId = null,
        int skip = 0,
        int take = 50,
        DateTime? agoraUtc = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retorna a pesquisa pronta para responder (valida Ativa/expirada).
    /// Inclui Perguntas -> Opções/Anexos conforme flags.
    /// </summary>
    Task<Pesquisas?> GetParaResponderAsync(
        int pesquisaId,
        bool incluirAnexosDaPergunta = true,
        CancellationToken ct = default
    );

    /// <summary>
    /// Busca por palavra-chave em Título/Descrição (case-insensitive).
    /// </summary>
    Task<List<PesquisaResumoDto>> BuscarAsync(
        int loginId,
        string termo,
        int max = 50,
        DateTime? agoraUtc = null,
        CancellationToken ct = default
    );

    Task<bool> AtualizarQRCodeUrlAsync(
        int pesquisaId,
        string? novaUrl,
        CancellationToken ct = default
    );
    Task<bool> AtualizarTemplateJsonAsync(
        int pesquisaId,
        string? templateJson,
        CancellationToken ct = default
    );
    Task<bool> AtualizarAtivaAsync(int pesquisaId, bool ativa, CancellationToken ct = default);
    Task<bool> TocarDataAtualizacaoAsync(
        int pesquisaId,
        DateTime? quandoUtc = null,
        CancellationToken ct = default
    );
}

/// <summary>
/// DTO leve para cards/listas.
/// </summary>
public sealed class PesquisaResumoDto
{
    public int PesquisaId { get; init; }
    public string Titulo { get; init; } = "";
    public string? Descricao { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime? DataAtualizacao { get; init; }
    public bool Ativa { get; init; }
    public bool TemLimitadorTempo { get; init; }
    public DateTime? DataFechamento { get; init; }
    public bool IsExpirada { get; init; }
    public int QtdPerguntas { get; init; }
    public int QtdRespostas { get; init; }
}
}
