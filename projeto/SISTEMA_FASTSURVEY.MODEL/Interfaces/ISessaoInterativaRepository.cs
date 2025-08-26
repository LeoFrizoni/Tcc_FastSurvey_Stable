#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ISessaoInterativaRepository : IRepository<SessoesInterativas>
    {
        Task<SessoesInterativas?> ObterPorCodigoAcessoAsync(
            string codigoAcesso,
            CancellationToken ct = default
        );
        Task<SessoesInterativas?> ObterPorPesquisaIdAsync(
            int pesquisaId,
            CancellationToken ct = default
        );

        // OPCIONAL: quando você quiser garantir que é a ativa mais recente
        Task<SessoesInterativas?> ObterAtivaMaisRecentePorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        );

        Task<bool> ExisteSessaoAtivaAsync(int pesquisaId, CancellationToken ct = default);
        Task<IEnumerable<SessoesInterativas>> ObterSessoesAtivasAsync(
            CancellationToken ct = default
        );
        Task<IEnumerable<SessoesInterativas>> ObterSessoesPorUsuarioAsync(
            int loginId,
            CancellationToken ct = default
        );
    }
}
