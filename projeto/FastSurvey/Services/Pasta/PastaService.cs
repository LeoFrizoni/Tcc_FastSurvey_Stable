#nullable enable
using FASTSURVEY.Dtos.Pastas;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

// ALIASES coerentes com o scaffold atual (classes no PLURAL)
using PastaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Pastas;
using PesquisaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Pesquisas;

namespace FASTSURVEY.Services.Pasta
{
    public class PastaService : IPastaService
    {
        private readonly FastSurveyContext _ctx;

        public PastaService(FastSurveyContext ctx) => _ctx = ctx;

        private static PastaResponse Map(PastaEntity p) =>
            new PastaResponse { PastaId = p.Pastaid, Nome = p.Nome };

        public async Task<List<PastaResponse>> ListarAsync(int loginId, CancellationToken ct = default)
        {
            var list = await _ctx.Set<PastaEntity>()
                                 .AsNoTracking()
                                 .Where(p => p.Loginid == loginId)
                                 .OrderBy(p => p.Nome)
                                 .ToListAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task<PastaResponse> CriarAsync(CriarPastaRequest req, CancellationToken ct = default)
        {
            var nome = (req.Nome ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome inv�lido.");
            if (nome.Length > 60)
                throw new ArgumentException("Nome excede 60 caracteres.");

            // Evita duplicidade por (LoginId, Nome)
            var existe = await _ctx.Set<PastaEntity>()
                                   .AnyAsync(p => p.Loginid == req.LoginId && p.Nome == nome, ct);
            if (existe)
                throw new InvalidOperationException("J� existe uma pasta com esse nome para este usu�rio.");

            var entity = new PastaEntity { Loginid = req.LoginId, Nome = nome };
            _ctx.Set<PastaEntity>().Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return Map(entity);
        }

        public async Task<bool> RenomearAsync(int pastaId, int loginId, string novoNome, CancellationToken ct = default)
        {
            var pasta = await _ctx.Set<PastaEntity>()
                                  .FirstOrDefaultAsync(p => p.Pastaid == pastaId && p.Loginid == loginId, ct);
            if (pasta == null) return false;

            var nome = (novoNome ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 60) return false;

            var conflito = await _ctx.Set<PastaEntity>()
                                     .AnyAsync(p => p.Loginid == loginId &&
                                                    p.Pastaid != pastaId &&
                                                    p.Nome == nome, ct);
            if (conflito) return false;

            pasta.Nome = nome;
            await _ctx.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ExcluirAsync(int pastaId, int loginId, CancellationToken ct = default)
        {
            var pasta = await _ctx.Set<PastaEntity>()
                                  .FirstOrDefaultAsync(p => p.Pastaid == pastaId && p.Loginid == loginId, ct);
            if (pasta == null) return false;

            // Desvincula pesquisas antes de remover a pasta
            var pesquisas = await _ctx.Set<PesquisaEntity>()
                                      .Where(x => x.Pastaid == pastaId && x.Loginid == loginId)
                                      .ToListAsync(ct);

            foreach (var pesq in pesquisas)
                pesq.Pastaid = null;

            _ctx.Set<PastaEntity>().Remove(pasta);
            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
