#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IPastaRepository : IRepository<Pastas>
    {
        Task<List<Pastas>> ListarPorLoginAsync(int loginId, CancellationToken ct = default);

        // Conveniências:
        Task<Pastas?> ObterPorNomeAsync(int loginId, string nome, CancellationToken ct = default);
        Task<bool> ExisteComNomeAsync(int loginId, string nome, CancellationToken ct = default);
        Task<int> CountPorLoginAsync(int loginId, CancellationToken ct = default);

        // Paginação/consulta:
        Task<List<Pastas>> ListarPorLoginAsync(
            int loginId,
            int skip,
            int take,
            CancellationToken ct = default
        );
        IQueryable<Pastas> QueryPorLogin(int loginId);

        // Operações sem SaveChanges (UoW decide):
        Task<Pastas> CriarAsync(int loginId, string nome, CancellationToken ct = default);
        Task<int> RenomearAsync(int pastaId, string novoNome, CancellationToken ct = default);
        Task<int> ExcluirPorIdAsync(int pastaId, CancellationToken ct = default);
    }
}
