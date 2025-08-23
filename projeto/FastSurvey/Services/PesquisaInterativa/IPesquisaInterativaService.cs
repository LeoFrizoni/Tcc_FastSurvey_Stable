using FASTSURVEY.Controllers;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public interface IPesquisaInterativaService
    {
        Task<ServiceResult<SessaoInterativaResponse>> IniciarSessaoAsync(PesquisaInterativaRequest request, CancellationToken ct = default);
        Task<ServiceResult<ParticipanteResponse>> EntrarSessaoAsync(EntrarSessaoRequest request, CancellationToken ct = default);
        Task<ServiceResult<SessaoInterativaResponse>> ObterSessaoAsync(string codigo, CancellationToken ct = default);
        Task<ServiceResult<RespostaInterativaResponse>> ResponderPerguntaAsync(RespostaInterativaRequest request, CancellationToken ct = default);
        Task<ServiceResult<object>> ObterResultadosTempoRealAsync(string sessaoId, CancellationToken ct = default);
        Task<ServiceResult<bool>> FinalizarSessaoAsync(FinalizarSessaoRequest request, CancellationToken ct = default);
        Task<ServiceResult<object>> ObterStatusSessaoAsync(string sessaoId, CancellationToken ct = default);
        Task<ServiceResult<List<ParticipanteResponse>>> ObterParticipantesAsync(string sessaoId, CancellationToken ct = default);
    }

    public class SessaoInterativaResponse
    {
        public string SessaoId { get; set; } = string.Empty;
        public string CodigoAcesso { get; set; } = string.Empty;
        public int PesquisaId { get; set; }
        public string TituloPesquisa { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? IniciadaEm { get; set; }
        public DateTime? FinalizadaEm { get; set; }
        public int TotalParticipantes { get; set; }
        public List<PerguntaInterativaResponse> Perguntas { get; set; } = new();
    }

    public class PerguntaInterativaResponse
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public int Ordem { get; set; }
        public List<OpcaoInterativaResponse> Opcoes { get; set; } = new();
    }

    public class OpcaoInterativaResponse
    {
        public int OpcaoId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }

    public class ParticipanteResponse
    {
        public int ParticipanteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string SessaoId { get; set; } = string.Empty;
        public DateTime EntrouEm { get; set; }
        public bool Ativo { get; set; }
    }

    public class RespostaInterativaResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int? RespostaId { get; set; }
        public object? ResultadosTempoReal { get; set; }
    }
}
