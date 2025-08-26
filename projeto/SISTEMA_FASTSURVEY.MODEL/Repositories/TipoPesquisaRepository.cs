#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoPesquisaRepository : Repository<TipoPesquisa>, ITipoPesquisaRepository
    {
        public TipoPesquisaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<TipoPesquisa>> ListarAtivosAsync(CancellationToken ct = default)
        {
            // Melhor para dropdowns: ordena por nome; cai para Id se nome vier nulo.
            return await _set.AsNoTracking()
                .Where(tp => !tp.Desabilitado)
                .OrderBy(tp => tp.TipoPesquisa1 ?? string.Empty)
                .ThenBy(tp => tp.TipoPesquisaId)
                .ToListAsync(ct);
        }

        public async Task<TipoPesquisa?> ObterAsync(
            int id,
            bool somenteAtivo = true,
            CancellationToken ct = default
        )
        {
            var q = _set.AsNoTracking().Where(tp => tp.TipoPesquisaId == id);
            if (somenteAtivo)
                q = q.Where(tp => !tp.Desabilitado);
            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<TipoPesquisa?> ObterPorNomeAsync(
            string nome,
            bool somenteAtivo = true,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            // Se estiver usando Npgsql, pode trocar por ILike:
            // return await _set.AsNoTracking()
            //     .Where(tp => !somenteAtivo || !tp.Desabilitado)
            //     .FirstOrDefaultAsync(tp => EF.Functions.ILike(tp.TipoPesquisa1!, nome), ct);

            nome = nome.Trim().ToLower();
            var q = _set.AsNoTracking()
                .Where(tp => tp.TipoPesquisa1 != null && tp.TipoPesquisa1.ToLower() == nome);
            if (somenteAtivo)
                q = q.Where(tp => !tp.Desabilitado);
            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExisteAsync(
            int id,
            bool somenteAtivo = true,
            CancellationToken ct = default
        )
        {
            var q = _set.AsNoTracking().Where(tp => tp.TipoPesquisaId == id);
            if (somenteAtivo)
                q = q.Where(tp => !tp.Desabilitado);
            return await q.AnyAsync(ct);
        }
    }
}
