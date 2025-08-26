#nullable enable
using System.Linq; // Where/OrderBy/ThenBy/Skip/Take
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class ParticipanteSessaoRepository
        : Repository<ParticipantesSessao>,
            IParticipanteSessaoRepository
    {
        public ParticipanteSessaoRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<ParticipantesSessao?> ObterPorNomeESessaoAsync(
            string nome,
            string sessaoId,
            CancellationToken ct = default
        )
        {
            // Comparação exata mantida (contrato atual). Normalizamos espaços.
            var nomeTrim = nome?.Trim() ?? string.Empty;
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .Where(p =>
                    p.SaiuEm == null && p.SessaoId == sessaoTrim && p.NomeParticipante == nomeTrim
                )
                .OrderByDescending(p => p.EntrouEm) // se houver duplicatas, pega a mais recente
                .ThenByDescending(p => p.ParticipanteId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<ParticipantesSessao>> ObterParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim && p.SaiuEm == null)
                .OrderBy(p => p.EntrouEm)
                .ThenBy(p => p.ParticipanteId)
                .ToListAsync(ct);
        }

        public async Task<int> ContarParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .CountAsync(p => p.SessaoId == sessaoTrim && p.SaiuEm == null, ct);
        }

        public async Task<List<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim)
                .OrderBy(p => p.EntrouEm)
                .ThenBy(p => p.ParticipanteId)
                .ToListAsync(ct);
        }

        public async Task<List<ParticipantesSessao>> ObterParticipantesPorSessaoAsync(
            string sessaoId,
            int skip,
            int take,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;
            if (skip < 0)
                skip = 0;
            if (take <= 0)
                take = 50;

            return await _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim)
                .OrderBy(p => p.EntrouEm)
                .ThenBy(p => p.ParticipanteId)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public IQueryable<ParticipantesSessao> QueryPorSessao(string sessaoId)
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim)
                .OrderBy(p => p.EntrouEm)
                .ThenBy(p => p.ParticipanteId);
        }

        // ===== Operações sem SaveChanges (UoW decide) =====

        public async Task<ParticipantesSessao> RegistrarEntradaAsync(
            string sessaoId,
            string nome,
            DateTime entrouEmUtc,
            CancellationToken ct = default
        )
        {
            // Garante que será persistido como UTC (coerente com timestamptz)
            var utc = DateTime.SpecifyKind(entrouEmUtc, DateTimeKind.Utc);

            var entity = new ParticipantesSessao
            {
                SessaoId = (sessaoId ?? string.Empty).Trim(),
                NomeParticipante = (nome ?? string.Empty).Trim(),
                EntrouEm = utc,
                SaiuEm = null,
            };

            await _set.AddAsync(entity, ct);
            return entity; // SaveChangesAsync é responsabilidade do Service/UoW
        }

        public async Task<int> MarcarSaidaParticipanteAsync(
            int participanteId,
            DateTime saiuEmUtc,
            CancellationToken ct = default
        )
        {
            var utc = DateTime.SpecifyKind(saiuEmUtc, DateTimeKind.Utc);

            // EF Core 7+: atualiza direto no banco, sem materializar
            var affected = await _set.Where(p =>
                    p.ParticipanteId == participanteId && p.SaiuEm == null
                )
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.SaiuEm, utc), ct);

            // Não requer SaveChangesAsync; já executado no banco
            return affected;

            // Versão antiga (carregando em memória):
            // var participante = await _set.FirstOrDefaultAsync(
            //     p => p.ParticipanteId == participanteId && p.SaiuEm == null, ct);
            // if (particiante is null) return 0;
            // participante.SaiuEm = utc;
            // return 1;
        }

        public async Task<int> MarcarSaidaParticipantesAsync(
            string sessaoId,
            DateTime saiuEmUtc,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;
            var utc = DateTime.SpecifyKind(saiuEmUtc, DateTimeKind.Utc);

            // Atualiza em massa no banco (mais performático que materializar)
            var affected = await _set.Where(p => p.SessaoId == sessaoTrim && p.SaiuEm == null)
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.SaiuEm, utc), ct);

            return affected;

            // Versão antiga (materializando):
            // var participantes = await _set
            //     .Where(p => p.SessaoId == sessaoTrim && p.SaiuEm == null)
            //     .ToListAsync(ct);
            // foreach (var p in participantes) p.SaiuEm = utc;
            // return participantes.Count;
        }
    }
}
