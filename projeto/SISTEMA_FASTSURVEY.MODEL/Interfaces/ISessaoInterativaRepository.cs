using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ISessaoInterativaRepository : IRepository<SessoesInterativas>
    {
        Task<SessoesInterativas?> ObterPorCodigoAcessoAsync(string codigoAcesso);
        Task<SessoesInterativas?> ObterPorPesquisaIdAsync(int pesquisaId);
        Task<bool> ExisteSessaoAtivaAsync(int pesquisaId);
        Task<IEnumerable<SessoesInterativas>> ObterSessoesAtivasAsync();
        Task<IEnumerable<SessoesInterativas>> ObterSessoesPorUsuarioAsync(int loginId);
    }
}
