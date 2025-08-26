// FASTSURVEY/Services/Opcoes/IOpcaoPerguntaService.cs
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Opcoes
{
    public interface IOpcaoPerguntaService
    {
        Task<ServiceResult<OpcaoPerguntaResponse>> CriarAsync(
            int perguntaId,
            OpcaoPerguntaRequest req,
            CancellationToken ct = default
        );

        Task<ServiceResult<OpcaoPerguntaResponse>> ObterPorIdAsync(
            int opcaoId,
            CancellationToken ct = default
        );

        Task<ServiceResult<IReadOnlyList<OpcaoPerguntaResponse>>> ListarPorPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarAsync(
            OpcaoPerguntaUpdateRequest req,
            CancellationToken ct = default
        );

        Task<ServiceResult<bool>> RemoverAsync(int opcaoId, CancellationToken ct = default);
    }
}
