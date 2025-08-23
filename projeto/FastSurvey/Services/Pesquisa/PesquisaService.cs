#nullable enable
using FASTSURVEY.Dtos.Pesquisas;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Text.Json;

namespace FASTSURVEY.Services.Pesquisa
{
    public class PesquisaService : IPesquisaService
    {
        private readonly FastSurveyContext _ctx;

        public PesquisaService(FastSurveyContext ctx) => _ctx = ctx;

        // ------------------ CREATE ------------------
        public async Task<int> CriarAsync(CriarPesquisaRequest req, CancellationToken ct = default)
        {
            // Valida JSON do template
            string template = string.IsNullOrWhiteSpace(req.TemplateJson) ? "[]"
                                : req.TemplateJson.Trim();

            try { JsonDocument.Parse(template); }
            catch { template = "[]"; }

            var entity = new Pesquisas
            {
                Loginid = req.LoginId,
                Tipopesquisaid = req.TipoPesquisaId,
                Pastaid = req.PastaId,
                Titulo = req.Titulo,
                Descricao = req.Descricao ?? string.Empty,
                Templatejson = template,
                Qrcodeurl = req.QrCodeUrl,   // pode ser nulo
                Datacriacao = DateTime.UtcNow,
                Dataatualizacao = null,
                // Novas funcionalidades
                Temlimitadortempo = req.TemLimitadorTempo,
                Datafechamento = req.DataFechamento,
                Isinterativa = req.IsInterativa,
                PermiteRespostasAnonimas = req.PermiteRespostasAnonimas,
                LimiteRespostas = req.LimiteRespostas,
                Ativa = true
            };

            _ctx.Pesquisas.Add(entity);
            await _ctx.SaveChangesAsync(ct);
            return entity.Pesquisaid;
        }

        // ------------------ UPDATE (PUT) ------------------
        public async Task<bool> AtualizarAsync(int id, AtualizarPesquisaRequest req, CancellationToken ct = default)
        {
            var entity = await _ctx.Pesquisas
                .FirstOrDefaultAsync(x => x.Pesquisaid == id, ct);
            if (entity is null) return false;

            entity.Titulo = req.Titulo;
            entity.Descricao = req.Descricao ?? entity.Descricao;
            entity.Pastaid = req.PastaId ?? entity.Pastaid;
            entity.Tipopesquisaid = req.TipoPesquisaId ?? entity.Tipopesquisaid;
            entity.Qrcodeurl = req.QrCodeUrl ?? entity.Qrcodeurl;
            
            // Atualizar novas funcionalidades
            if (req.TemLimitadorTempo.HasValue) entity.Temlimitadortempo = req.TemLimitadorTempo.Value;
            if (req.DataFechamento.HasValue) entity.Datafechamento = req.DataFechamento;
            if (req.IsInterativa.HasValue) entity.Isinterativa = req.IsInterativa.Value;
            if (req.PermiteRespostasAnonimas.HasValue) entity.PermiteRespostasAnonimas = req.PermiteRespostasAnonimas.Value;
            if (req.LimiteRespostas.HasValue) entity.LimiteRespostas = req.LimiteRespostas;

            entity.Dataatualizacao = DateTime.UtcNow;

            await _ctx.SaveChangesAsync(ct);
            return true;
        }

        // ------------------ DELETE ------------------
        public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx.Pesquisas.FirstOrDefaultAsync(x => x.Pesquisaid == id, ct);
            if (entity is null) return false;

            _ctx.Pesquisas.Remove(entity);
            await _ctx.SaveChangesAsync(ct); // CASCADE conforme delta aplicado
            return true;
        }

        // ------------------ GET BY ID ------------------
        public async Task<PesquisaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var q = await _ctx.Pesquisas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Pesquisaid == id, ct);

            if (q is null) return null;

            return new PesquisaResponse
            {
                PesquisaId = q.Pesquisaid,
                Titulo = q.Titulo,
                Descricao = q.Descricao,
                LoginId = q.Loginid,
                TipoPesquisaId = q.Tipopesquisaid,
                PastaId = q.Pastaid,
                TemplateJson = q.Templatejson ?? "[]",
                QrCodeUrl = q.Qrcodeurl,
                DataCriacao = q.Datacriacao,
                DataAtualizacao = q.Dataatualizacao
            };
        }

        // ------------------ LIST / SEARCH (PAGINADO) ------------------
        public async Task<PagedResult<PesquisaListItemResponse>> ListarAsync(PesquisaFiltroRequest filtro, CancellationToken ct = default)
        {
            int page = Math.Max(1, filtro.Page ?? 1);
            int pageSize = Math.Clamp(filtro.PageSize ?? 20, 1, 200);

            var query = _ctx.Pesquisas.AsNoTracking().AsQueryable();

            if (filtro.LoginId.HasValue)
                query = query.Where(x => x.Loginid == filtro.LoginId.Value);

            if (filtro.PastaId.HasValue)
                query = query.Where(x => x.Pastaid == filtro.PastaId.Value);

            if (filtro.TipoPesquisaId.HasValue)
                query = query.Where(x => x.Tipopesquisaid == filtro.TipoPesquisaId.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var b = filtro.Busca.Trim().ToLower();
                query = query.Where(x =>
                    x.Titulo.ToLower().Contains(b) ||
                    (x.Descricao != null && x.Descricao.ToLower().Contains(b))
                );
            }

            int total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.Dataatualizacao ?? x.Datacriacao)
                .ThenByDescending(x => x.Pesquisaid)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new PesquisaListItemResponse
                {
                    PesquisaId = q.Pesquisaid,
                    Titulo = q.Titulo,
                    Descricao = q.Descricao,
                    LoginId = q.Loginid,
                    TipoPesquisaId = q.Tipopesquisaid,
                    PastaId = q.Pastaid,
                    QrCodeUrl = q.Qrcodeurl,
                    DataCriacao = q.Datacriacao,
                    DataAtualizacao = q.Dataatualizacao,
                    TemLimitadorTempo = q.Temlimitadortempo,
                    DataFechamento = q.Datafechamento,
                    IsInterativa = q.Isinterativa,
                    PermiteRespostasAnonimas = q.PermiteRespostasAnonimas,
                    LimiteRespostas = q.LimiteRespostas,
                    Ativa = q.Ativa
                })
                .ToListAsync(ct);

            return new PagedResult<PesquisaListItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }

        // ------------------ PATCH TEMPLATE ------------------
        public async Task<bool> AtualizarTemplateAsync(int id, string templateJson, CancellationToken ct = default)
        {
            var entity = await _ctx.Pesquisas.FirstOrDefaultAsync(x => x.Pesquisaid == id, ct);
            if (entity is null) return false;

            if (string.IsNullOrWhiteSpace(templateJson)) templateJson = "[]";
            try { JsonDocument.Parse(templateJson); }
            catch { templateJson = "[]"; }

            entity.Templatejson = templateJson;
            entity.Dataatualizacao = DateTime.UtcNow;

            await _ctx.SaveChangesAsync(ct);
            return true;
        }

        // ================== NOVOS M�TODOS (exigidos pela interface) ==================

        // Lista simples por usu�rio � usada por GET /api/pesquisas/usuario/{loginId}
        public async Task<List<PesquisaListItemResponse>> ListarPorLoginAsync(int loginId, CancellationToken ct = default)
        {
            var list = await _ctx.Pesquisas
                .AsNoTracking()
                .Where(x => x.Loginid == loginId)
                .OrderByDescending(x => x.Dataatualizacao ?? x.Datacriacao)
                .ThenByDescending(x => x.Pesquisaid)
                .Select(q => new PesquisaListItemResponse
                {
                    PesquisaId = q.Pesquisaid,
                    Titulo = q.Titulo,
                    Descricao = q.Descricao,
                    LoginId = q.Loginid,
                    TipoPesquisaId = q.Tipopesquisaid,
                    PastaId = q.Pastaid,
                    QrCodeUrl = q.Qrcodeurl,
                    DataCriacao = q.Datacriacao,
                    DataAtualizacao = q.Dataatualizacao
                })
                .ToListAsync(ct);

            return list;
        }

        // Define/atualiza a pasta � usada por PATCH /api/pesquisas/{id}/mover-pasta
        public async Task<bool> DefinirPastaAsync(int pesquisaId, int? pastaId, CancellationToken ct = default)
        {
            var entity = await _ctx.Pesquisas.FirstOrDefaultAsync(x => x.Pesquisaid == pesquisaId, ct);
            if (entity is null) return false;

            entity.Pastaid = pastaId;               // null = �Sem pasta�
            entity.Dataatualizacao = DateTime.UtcNow;

            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
