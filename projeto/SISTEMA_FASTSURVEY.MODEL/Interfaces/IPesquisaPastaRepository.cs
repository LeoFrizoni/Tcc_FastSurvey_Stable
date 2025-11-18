#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IPesquisaPastaRepository : IRepository<PesquisasPastas>
    {
        Task<PesquisasPastas?> ObterPorPesquisaAsync(int pesquisaId, CancellationToken ct = default);

        Task<int> RemoverPorPesquisaAsync(int pesquisaId, CancellationToken ct = default);
    }
}
