#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.PesquisaPasta
{
    public class PesquisaPastaService : IPesquisaPastaService
    {
        private readonly FastSurveyContext _ctx;

        public PesquisaPastaService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<int?> ObterPastaIdAsync(int pesquisaId, CancellationToken ct = default)
        {
            return await _ctx.PesquisasPastas
                .AsNoTracking()
                .Where(x => x.PesquisaId == pesquisaId)
                .Select(x => (int?)x.PastaId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> DefinirPastaAsync(
            int pesquisaId,
            int loginId,
            int? pastaId,
            CancellationToken ct = default
        )
        {
            var vinculo = await _ctx.PesquisasPastas
                .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);

            if (!pastaId.HasValue)
            {
                if (vinculo is not null)
                    _ctx.PesquisasPastas.Remove(vinculo);
                await _ctx.SaveChangesAsync(ct);
                return true;
            }

            var pastaExiste = await _ctx.Pastas
                .AsNoTracking()
                .AnyAsync(p => p.PastaId == pastaId && p.LoginId == loginId, ct);
            if (!pastaExiste)
                return false;

            if (vinculo is null)
            {
                vinculo = new PesquisasPastas
                {
                    PesquisaId = pesquisaId,
                    PastaId = pastaId.Value,
                    VinculadaEm = DateTime.UtcNow,
                };
                _ctx.PesquisasPastas.Add(vinculo);
            }
            else if (vinculo.PastaId != pastaId.Value)
            {
                vinculo.PastaId = pastaId.Value;
                vinculo.VinculadaEm = DateTime.UtcNow;
                _ctx.PesquisasPastas.Update(vinculo);
            }

            await _ctx.SaveChangesAsync(ct);
            return true;
        }

        public async Task<Dictionary<int, int?>> MapearPastaPorPesquisaAsync(
            IEnumerable<int> pesquisaIds,
            CancellationToken ct = default
        )
        {
            var ids = pesquisaIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new();

            var vinculos = await _ctx.PesquisasPastas
                .AsNoTracking()
                .Where(x => ids.Contains(x.PesquisaId))
                .Select(x => new { x.PesquisaId, x.PastaId })
                .ToListAsync(ct);

            var map = ids.ToDictionary(id => id, _ => (int?)null);
            foreach (var vinculo in vinculos)
                map[vinculo.PesquisaId] = vinculo.PastaId;

            return map;
        }
    }
}
