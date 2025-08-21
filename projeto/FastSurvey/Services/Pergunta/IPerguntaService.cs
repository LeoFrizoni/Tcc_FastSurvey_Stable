using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Dtos.Perguntas.Base;
using FASTSURVEY.Dtos.Perguntas.Discursiva;
using FASTSURVEY.Dtos.Perguntas.Objetiva;
using FASTSURVEY.Dtos.Perguntas.Multipla;
using FASTSURVEY.Dtos.Opcoes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Pergunta
{
    public interface IPerguntaService
    {
        // -------------------- CRUD Perguntas (por tipo) --------------------
        Task<ServiceResult<PerguntaResponse>> CriarDiscursivaAsync(
            CriarPerguntaDiscursivaRequest req, CancellationToken ct = default);

        Task<ServiceResult<PerguntaResponse>> CriarObjetivaAsync(
            CriarPerguntaObjetivaRequest req, CancellationToken ct = default);

        Task<ServiceResult<PerguntaResponse>> CriarMultiplaAsync(
            CriarPerguntaMultiplaRequest req, CancellationToken ct = default);

        Task<ServiceResult<PerguntaResponse>> AtualizarDiscursivaAsync(
            AtualizarPerguntaDiscursivaRequest req, CancellationToken ct = default);

        Task<ServiceResult<PerguntaResponse>> AtualizarObjetivaAsync(
            AtualizarPerguntaObjetivaRequest req, CancellationToken ct = default);

        Task<ServiceResult<PerguntaResponse>> AtualizarMultiplaAsync(
            AtualizarPerguntaMultiplaRequest req, CancellationToken ct = default);

        Task<ServiceResult<bool>> ExcluirAsync(
            int perguntaId, CancellationToken ct = default);

        // -------------------- Consulta --------------------
        Task<ServiceResult<PerguntaResponse>> ObterPorIdAsync(
            int perguntaId, CancellationToken ct = default);

        Task<ServiceResult<IReadOnlyList<PerguntaResponse>>> ListarPorPesquisaAsync(
            int pesquisaId, CancellationToken ct = default);

        // -------------------- Gabarito --------------------
        // permitirApenasUma = true para Objetiva, false para Múltipla (N corretas)
        Task<ServiceResult<bool>> DefinirGabaritoAsync(
            int perguntaId, IEnumerable<int> opcaoIds, bool permitirApenasUma, CancellationToken ct = default);

        // -------------------- Ordenação --------------------
        Task<ServiceResult<bool>> ReordenarAsync(
            int pesquisaId, IReadOnlyList<int> perguntaIdsNaOrdem, CancellationToken ct = default);

        // -------------------- Opções --------------------
        Task<ServiceResult<OpcaoPerguntaResponse>> AdicionarOpcaoAsync(
            int perguntaId, OpcaoPerguntaRequest req, CancellationToken ct = default);

        Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarOpcaoAsync(
            OpcaoPerguntaUpdateRequest req, CancellationToken ct = default);

        Task<ServiceResult<bool>> RemoverOpcaoAsync(
            int opcaoId, bool softDelete = true, CancellationToken ct = default);

        Task<ServiceResult<bool>> ReordenarOpcoesAsync(
            int perguntaId, IReadOnlyList<int> opcaoIdsNaOrdem, CancellationToken ct = default);
    }
}
