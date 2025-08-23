#nullable enable
using FASTSURVEY.Services.Result;

// Alias para casar com a classe gerada pelo scaffold (tabela opcoespergunta)
using OpcaoPergunta = SISTEMA_FASTSURVEY.MODEL.Models.Opcoespergunta;

namespace FASTSURVEY.Services.Opcoes
{
    public interface IOpcaoPerguntaService
    {
        Task<ServiceResult<OpcaoPergunta>> CriarAsync(
            int perguntaId,
            string texto,
            bool? correta = null,
            int? ordem = null,
            bool? ativa = null,
            CancellationToken ct = default
        );

        Task<ServiceResult<OpcaoPergunta>> ObterPorIdAsync(
            int opcaoId,
            CancellationToken ct = default
        );

        Task<ServiceResult<List<OpcaoPergunta>>> ListarPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        Task<ServiceResult<OpcaoPergunta>> AtualizarAsync(
            int opcaoId,
            Action<OpcaoPergunta> applyUpdates,
            CancellationToken ct = default
        );

        Task<ServiceResult<bool>> RemoverAsync(
            int opcaoId,
            CancellationToken ct = default
        );
    }
}
