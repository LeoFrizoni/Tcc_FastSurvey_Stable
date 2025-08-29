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

        public async Task<ParticipantesSessao?> ObterParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParticipanteId == participanteId, ct);
        }

        public async Task<List<ParticipantesSessao>> ObterParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;

            return await _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim && p.SaiuEm == null && p.Ativo)
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
                .CountAsync(p => p.SessaoId == sessaoTrim && p.SaiuEm == null && p.Ativo, ct);
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

        public async Task<List<ParticipantesSessao>> ObterParticipantesPorPeriodoAsync(
            string sessaoId,
            DateTime inicio,
            DateTime fim,
            CancellationToken ct = default
        )
        {
            var sessaoTrim = sessaoId?.Trim() ?? string.Empty;
            var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
            var fimUtc = DateTime.SpecifyKind(fim, DateTimeKind.Utc);

            return await _set.AsNoTracking()
                .Where(p => p.SessaoId == sessaoTrim && p.EntrouEm >= inicioUtc && p.EntrouEm <= fimUtc)
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
                Ativo = true,
            };

            await _set.AddAsync(entity, ct);
            return entity; // SaveChangesAsync é responsabilidade do Service/UoW
        }

        public async Task<bool> MarcarSaidaParticipanteAsync(
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

            return affected > 0;
        }

        public async Task<bool> AtualizarParticipanteAsync(
            int participanteId,
            string nome,
            CancellationToken ct = default
        )
        {
            var nomeTrim = (nome ?? string.Empty).Trim();

            var affected = await _set.Where(p => p.ParticipanteId == participanteId)
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.NomeParticipante, nomeTrim), ct);

            return affected > 0;
        }

        public async Task<bool> RemoverParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            var participante = await _set.FirstOrDefaultAsync(p => p.ParticipanteId == participanteId, ct);
            if (participante == null)
                return false;

            _set.Remove(participante);
            return true;
        }

        public async Task<bool> AtivarParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            var affected = await _set.Where(p => p.ParticipanteId == participanteId)
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.Ativo, true), ct);

            return affected > 0;
        }

        public async Task<bool> DesativarParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            var affected = await _set.Where(p => p.ParticipanteId == participanteId)
                .ExecuteUpdateAsync(updates => updates.SetProperty(p => p.Ativo, false), ct);

            return affected > 0;
        }
    }
}
