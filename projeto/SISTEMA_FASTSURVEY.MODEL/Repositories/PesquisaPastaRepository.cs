#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PesquisaPastaRepository : Repository<PesquisasPastas>, IPesquisaPastaRepository
    {
        public PesquisaPastaRepository(FastSurveyContext context)
            : base(context) { }

        public Task<PesquisasPastas?> ObterPorPesquisaAsync(int pesquisaId, CancellationToken ct = default) =>
            _set.AsNoTracking().FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);

        public Task<int> RemoverPorPesquisaAsync(int pesquisaId, CancellationToken ct = default) =>
            _set.Where(x => x.PesquisaId == pesquisaId).ExecuteDeleteAsync(ct);
    }
}
