#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoPerguntaRepository : Repository<TipoPergunta>, ITipoPerguntaRepository
    {
        public TipoPerguntaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<TipoPergunta>> ListarAtivosAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(tp => !tp.Desabilitado)
                .OrderBy(tp => tp.TipoPergunta1 ?? string.Empty)
                .ToListAsync(ct);
        }

        public async Task<TipoPergunta?> ObterAsync(
            int id,
            bool somenteAtivo = true,
            CancellationToken ct = default
        )
        {
            var q = _set.AsNoTracking().Where(tp => tp.TipoPerguntaId == id);
            if (somenteAtivo)
                q = q.Where(tp => !tp.Desabilitado);
            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<TipoPergunta?> ObterPorNomeAsync(
            string nome,
            bool somenteAtivo = true,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            // Se estiver usando Npgsql, pode trocar pela versão com ILike:
            // return await _set.AsNoTracking()
            //      .Where(tp => !somenteAtivo || !tp.Desabilitado)
            //      .FirstOrDefaultAsync(tp => EF.Functions.ILike(tp.TipoPergunta1!, nome), ct);

            nome = nome.Trim();
            var q = _set.AsNoTracking()
                .Where(tp =>
                    tp.TipoPergunta1 != null && tp.TipoPergunta1.ToLower() == nome.ToLower()
                );
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
            var q = _set.AsNoTracking().Where(tp => tp.TipoPerguntaId == id);
            if (somenteAtivo)
                q = q.Where(tp => !tp.Desabilitado);
            return await q.AnyAsync(ct);
        }
    }
}
