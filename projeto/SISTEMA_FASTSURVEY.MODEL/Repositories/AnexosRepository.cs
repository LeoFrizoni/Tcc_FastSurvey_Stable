#nullable enable
using System.Linq; // importante para Where/OrderBy/Count/Any
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class AnexoRepository : Repository<AnexoEntity>, IAnexoRepository
    {
        public AnexoRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<List<AnexoEntity>> ListarPorPesquisaAsync(
            int PesquisaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(a => a.PesquisaId == PesquisaId)
                .OrderBy(a => a.AnexoId) // ordenação estável
                .ToListAsync(ct);
        }

        public async Task<List<AnexoEntity>> ListarPorPerguntaAsync(
            int PerguntaId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .Where(a => a.PerguntaId == PerguntaId)
                .OrderBy(a => a.AnexoId) // ordenação estável
                .ToListAsync(ct);
        }

        // ---------- Consultas componíveis ----------
        public IQueryable<AnexoEntity> QueryPorPesquisa(int pesquisaId) =>
            _set.AsNoTracking().Where(a => a.PesquisaId == pesquisaId).OrderBy(a => a.AnexoId);

        public IQueryable<AnexoEntity> QueryPorPergunta(int perguntaId) =>
            _set.AsNoTracking().Where(a => a.PerguntaId == perguntaId).OrderBy(a => a.AnexoId);

        // ---------- Auxiliares ----------
        public async Task<bool> ExisteNaPesquisaAsync(
            int pesquisaId,
            string nome,
            CancellationToken ct = default
        )
        {
            // comparação exata (mantida por compatibilidade)
            return await _set.AsNoTracking()
                .AnyAsync(a => a.PesquisaId == pesquisaId && a.Nome == nome, ct);
        }

        public async Task<bool> ExisteNaPesquisaInsensitiveAsync(
            int pesquisaId,
            string nome,
            CancellationToken ct = default
        )
        {
            // Variante para Postgres com Npgsql (case-insensitive)
            // Se não usar Npgsql, você pode trocar por ToLower() == ToLower(), ciente do custo.
            return await _set.AsNoTracking()
                .AnyAsync(a => a.PesquisaId == pesquisaId && EF.Functions.ILike(a.Nome, nome), ct);
        }

        public async Task<int> CountPorPesquisaAsync(int pesquisaId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking().CountAsync(a => a.PesquisaId == pesquisaId, ct);
        }
    }
}
