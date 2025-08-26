#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PesquisaRepository : Repository<Pesquisas>, IPesquisaRepository
    {
        public PesquisaRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<Pesquisas?> GetDetalheAsync(
            int pesquisaId,
            bool incluirPerguntas = true,
            bool incluirAnexos = false,
            bool incluirTipoPesquisa = false,
            bool incluirLogin = false,
            bool incluirPasta = false,
            bool incluirSessoes = false,
            CancellationToken ct = default
        )
        {
            IQueryable<Pesquisas> q = _set.AsNoTrackingWithIdentityResolution()
                .Where(p => p.PesquisaId == pesquisaId);

            if (incluirPerguntas)
            {
                q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.OpcoesPergunta);

                q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.Anexos);
                // Se quiser habilitar futuramente:
                // q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.TipoPergunta);
                // q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.Respostas);
            }

            if (incluirAnexos)
                q = q.Include(p => p.Anexos);

            if (incluirTipoPesquisa)
                q = q.Include(p => p.TipoPesquisa);

            if (incluirLogin)
                q = q.Include(p => p.Login);

            if (incluirPasta)
                q = q.Include(p => p.Pasta);

            if (incluirSessoes)
                q = q.Include(p => p.SessoesInterativas);

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<List<Pesquisas>> ListarPorLoginAsync(
            int loginId,
            int? pastaId = null,
            int? tipoPesquisaId = null,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 50;

            IQueryable<Pesquisas> q = _set.AsNoTracking().Where(p => p.LoginId == loginId);

            if (pastaId is not null)
                q = q.Where(p => p.PastaId == pastaId);
            if (tipoPesquisaId is not null)
                q = q.Where(p => p.TipoPesquisaId == tipoPesquisaId);

            q = q.OrderByDescending(p => p.DataAtualizacao ?? p.DataCriacao)
                .ThenByDescending(p => p.PesquisaId)
                .Skip(skip)
                .Take(take);

            return await q.ToListAsync(ct);
        }

        public async Task<List<PesquisaResumoDto>> ListarResumoPorLoginAsync(
            int loginId,
            int? pastaId = null,
            int? tipoPesquisaId = null,
            int skip = 0,
            int take = 50,
            DateTime? agoraUtc = null,
            CancellationToken ct = default
        )
        {
            if (take <= 0)
                take = 50;
            var now = (agoraUtc ?? DateTime.UtcNow);

            var q = _set.AsNoTracking().Where(p => p.LoginId == loginId);

            if (pastaId is not null)
                q = q.Where(p => p.PastaId == pastaId);
            if (tipoPesquisaId is not null)
                q = q.Where(p => p.TipoPesquisaId == tipoPesquisaId);

            return await q.OrderByDescending(p => p.DataAtualizacao ?? p.DataCriacao)
                .ThenByDescending(p => p.PesquisaId)
                .Skip(skip)
                .Take(take)
                .Select(p => new PesquisaResumoDto
                {
                    PesquisaId = p.PesquisaId,
                    Titulo = p.Titulo,
                    Descricao = p.Descricao,
                    DataCriacao = p.DataCriacao,
                    DataAtualizacao = p.DataAtualizacao,
                    Ativa = p.Ativa,
                    TemLimitadorTempo = p.TemLimitadorTempo,
                    DataFechamento = p.DataFechamento,
                    IsExpirada =
                        p.TemLimitadorTempo && p.DataFechamento != null && p.DataFechamento <= now,
                    QtdPerguntas = p.Perguntas.Count,
                    // Soma respostas das perguntas (sem alterar DB nem precisar navegar além de Perguntas->Respostas)
                    QtdRespostas = p.Perguntas.Select(pg => pg.Respostas.Count).Sum(),
                })
                .ToListAsync(ct);
        }

        public async Task<Pesquisas?> GetParaResponderAsync(
            int pesquisaId,
            bool incluirAnexosDaPergunta = true,
            CancellationToken ct = default
        )
        {
            var now = DateTime.UtcNow;

            var q = _set.AsNoTrackingWithIdentityResolution()
                .Where(p => p.PesquisaId == pesquisaId && p.Ativa);

            // Validação de expiração
            q = q.Where(p =>
                !p.TemLimitadorTempo || (p.DataFechamento == null || p.DataFechamento > now)
            );

            q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.OpcoesPergunta);

            if (incluirAnexosDaPergunta)
                q = q.Include(p => p.Perguntas).ThenInclude(pg => pg.Anexos);

            // Opcional, se quiser já ordenar pelo ID (mantendo sem campo novo)
            // q = q.Select(p => new Pesquisas {
            //    ... // se quisesse projetar
            // });

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<List<PesquisaResumoDto>> BuscarAsync(
            int loginId,
            string termo,
            int max = 50,
            DateTime? agoraUtc = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(termo))
                return new();

            var now = (agoraUtc ?? DateTime.UtcNow);
            termo = termo.Trim();

            return await _set.AsNoTracking()
                .Where(p =>
                    p.LoginId == loginId
                    && (
                        EF.Functions.ILike(p.Titulo, $"%{termo}%")
                        || EF.Functions.ILike(p.Descricao ?? "", $"%{termo}%")
                    )
                )
                .OrderByDescending(p => p.DataAtualizacao ?? p.DataCriacao)
                .ThenByDescending(p => p.PesquisaId)
                .Take(Math.Max(1, Math.Min(max, 200)))
                .Select(p => new PesquisaResumoDto
                {
                    PesquisaId = p.PesquisaId,
                    Titulo = p.Titulo,
                    Descricao = p.Descricao,
                    DataCriacao = p.DataCriacao,
                    DataAtualizacao = p.DataAtualizacao,
                    Ativa = p.Ativa,
                    TemLimitadorTempo = p.TemLimitadorTempo,
                    DataFechamento = p.DataFechamento,
                    IsExpirada =
                        p.TemLimitadorTempo && p.DataFechamento != null && p.DataFechamento <= now,
                    QtdPerguntas = p.Perguntas.Count,
                    QtdRespostas = p.Perguntas.Select(pg => pg.Respostas.Count).Sum(),
                })
                .ToListAsync(ct);
        }

        public async Task<bool> AtualizarQRCodeUrlAsync(
            int pesquisaId,
            string? novaUrl,
            CancellationToken ct = default
        )
        {
            var ent = await _set.FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (ent is null)
                return false;

            ent.QRCodeUrl = novaUrl ?? string.Empty;
            ent.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AtualizarTemplateJsonAsync(
            int pesquisaId,
            string? templateJson,
            CancellationToken ct = default
        )
        {
            var ent = await _set.FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (ent is null)
                return false;

            ent.TemplateJson = templateJson ?? string.Empty;
            ent.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AtualizarAtivaAsync(
            int pesquisaId,
            bool ativa,
            CancellationToken ct = default
        )
        {
            var ent = await _set.FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (ent is null)
                return false;

            ent.Ativa = ativa;
            ent.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> TocarDataAtualizacaoAsync(
            int pesquisaId,
            DateTime? quandoUtc = null,
            CancellationToken ct = default
        )
        {
            var ent = await _set.FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (ent is null)
                return false;

            ent.DataAtualizacao = quandoUtc ?? DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
