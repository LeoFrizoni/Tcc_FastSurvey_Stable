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
            new PastaResponse
            {
                PastaId = p.PastaId,
                Nome = p.Nome,
                LoginId = p.LoginId,
            };

        public async Task<List<PastaResponse>> ListarAsync(
            int loginId,
            CancellationToken ct = default
        )
        {
            var list = await _ctx.Set<PastaEntity>()
                .AsNoTracking()
                .Where(p => p.LoginId == loginId)
                .OrderBy(p => p.Nome)
                .ToListAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task<PastaResponse> CriarAsync(
            CriarPastaRequest req,
            CancellationToken ct = default
        )
        {
            var nome = (req.Nome ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome inválido.");
            if (nome.Length > 60)
                throw new ArgumentException("Nome excede 60 caracteres.");

            // Evita duplicidade por (LoginId, Nome) com normalização simples
            var exists = await _ctx.Set<PastaEntity>()
                .AnyAsync(p => p.LoginId == req.LoginId && p.Nome.ToUpper() == nome.ToUpper(), ct);
            if (exists)
                throw new InvalidOperationException(
                    "Já existe uma pasta com esse nome para este usuário."
                );

            var entity = new PastaEntity { LoginId = req.LoginId, Nome = nome };
            _ctx.Set<PastaEntity>().Add(entity);

            try
            {
                await _ctx.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // Caso tenha índice único no futuro, tratamos conflito aqui
                throw new InvalidOperationException(
                    "Não foi possível criar a pasta (conflito de nome).",
                    ex
                );
            }

            return Map(entity);
        }

        public async Task<bool> RenomearAsync(
            int pastaId,
            int loginId,
            string novoNome,
            CancellationToken ct = default
        )
        {
            var pasta = await _ctx.Set<PastaEntity>()
                .FirstOrDefaultAsync(p => p.PastaId == pastaId && p.LoginId == loginId, ct);
            if (pasta is null)
                return false;

            var nome = (novoNome ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 60)
                return false;

            var conflito = await _ctx.Set<PastaEntity>()
                .AnyAsync(
                    p =>
                        p.LoginId == loginId
                        && p.PastaId != pastaId
                        && p.Nome.ToUpper() == nome.ToUpper(),
                    ct
                );
            if (conflito)
                return false;

            pasta.Nome = nome;

            try
            {
                await _ctx.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> ExcluirAsync(
            int pastaId,
            int loginId,
            CancellationToken ct = default
        )
        {
            var pasta = await _ctx.Set<PastaEntity>()
                .FirstOrDefaultAsync(p => p.PastaId == pastaId && p.LoginId == loginId, ct);
            if (pasta is null)
                return false;

            // Desvincula pesquisas antes de remover a pasta
            var pesquisas = await _ctx.Set<PesquisaEntity>()
                .Where(x => x.PastaId == pastaId && x.LoginId == loginId)
                .ToListAsync(ct);

            foreach (var pesq in pesquisas)
                pesq.PastaId = null;

            _ctx.Set<PastaEntity>().Remove(pasta);
            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
