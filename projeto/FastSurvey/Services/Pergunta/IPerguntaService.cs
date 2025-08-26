#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Pergunta
{
    public interface IPerguntaService
    {
        // CREATE/UPDATE unificados
        Task<ServiceResult<PerguntaResponse>> CriarAsync(
            CriarPerguntaRequest req,
            CancellationToken ct = default
        );
        Task<ServiceResult<PerguntaResponse>> AtualizarAsync(
            AtualizarPerguntaRequest req,
            CancellationToken ct = default
        );

        // DELETE
        Task<ServiceResult<bool>> ExcluirAsync(int perguntaId, CancellationToken ct = default);

        // READ
        Task<ServiceResult<PerguntaResponse>> ObterPorIdAsync(
            int perguntaId,
            CancellationToken ct = default
        );
        Task<ServiceResult<IReadOnlyList<PerguntaResponse>>> ListarPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        );

        // GABARITO
        // permitirApenasUma = true para Objetiva, false para Múltipla
        Task<ServiceResult<bool>> DefinirGabaritoAsync(
            int perguntaId,
            IEnumerable<int> opcaoIds,
            bool permitirApenasUma,
            CancellationToken ct = default
        );

        // ORDENACAO de perguntas
        Task<ServiceResult<bool>> ReordenarAsync(
            int pesquisaId,
            IReadOnlyList<int> perguntaIdsNaOrdem,
            CancellationToken ct = default
        );

        // OPCOES
        Task<ServiceResult<OpcaoPerguntaResponse>> AdicionarOpcaoAsync(
            int perguntaId,
            OpcaoPerguntaRequest req,
            CancellationToken ct = default
        );
        Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarOpcaoAsync(
            OpcaoPerguntaUpdateRequest req,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> RemoverOpcaoAsync(
            int opcaoId,
            bool softDelete = true,
            CancellationToken ct = default
        );
        Task<ServiceResult<bool>> ReordenarOpcoesAsync(
            int perguntaId,
            IReadOnlyList<int> opcaoIdsNaOrdem,
            CancellationToken ct = default
        );
    }
}
