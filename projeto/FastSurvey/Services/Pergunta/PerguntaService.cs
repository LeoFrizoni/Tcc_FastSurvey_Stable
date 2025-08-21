using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Dtos.Perguntas.Base;
using FASTSURVEY.Dtos.Perguntas.Discursiva;
using FASTSURVEY.Dtos.Perguntas.Multipla;
using FASTSURVEY.Dtos.Perguntas.Objetiva;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpcaoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Opcoespergunta;
// Aliases pros tipos do scaffold (pluralizados)
using PerguntaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace FASTSURVEY.Services.Pergunta
{
    public class PerguntaService : IPerguntaService
    {
        private readonly FastSurveyContext _ctx;

        private const int TIPO_DISCURSIVA = 1;
        private const int TIPO_OBJETIVA = 2;
        private const int TIPO_MULTIPLA = 3;

        public PerguntaService(FastSurveyContext ctx) => _ctx = ctx;

        // -------------------- CREATE --------------------

        public async Task<ServiceResult<PerguntaResponse>> CriarDiscursivaAsync(
            CriarPerguntaDiscursivaRequest req, CancellationToken ct = default)
        {
            var entity = new PerguntaEntity
            {
                Pesquisaid = req.PesquisaId,
                Tipoperguntaid = TIPO_DISCURSIVA,
                Texto = req.Texto,
                Temgabarito = false,
                Permitemultiplaselecao = false,
                Ordem = req.Ordem
            };

            _ctx.Perguntas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            var resp = await MapPerguntaToResponse(entity, ct);
            return ServiceOk(resp);
        }

        public async Task<ServiceResult<PerguntaResponse>> CriarObjetivaAsync(
            CriarPerguntaObjetivaRequest req, CancellationToken ct = default)
        {
            if (req.Opcoes == null || req.Opcoes.Count < 2)
                return ServiceFail<PerguntaResponse>("Pergunta objetiva exige pelo menos 2 opções.");

            var entity = new PerguntaEntity
            {
                Pesquisaid = req.PesquisaId,
                Tipoperguntaid = TIPO_OBJETIVA,
                Texto = req.Texto,
                Temgabarito = req.TemGabarito,
                Permitemultiplaselecao = false,
                Ordem = req.Ordem
            };

            foreach (var o in req.Opcoes)
            {
                entity.Opcoespergunta.Add(new OpcaoEntity
                {
                    Pergunta = entity, // EF seta Perguntaid
                    Texto = o.Texto,
                    Correta = o.Correta ?? false
                });
            }

            _ctx.Perguntas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            await _ctx.Entry(entity).Collection(p => p.Opcoespergunta).LoadAsync(ct);

            if (req.TemGabarito)
            {
                if (req.OpcaoCorretaId.HasValue)
                {
                    var correta = entity.Opcoespergunta.FirstOrDefault(x => x.Opcaoid == req.OpcaoCorretaId.Value);
                    if (correta == null)
                        return ServiceFail<PerguntaResponse>("OpcaoCorretaId não pertence à pergunta.");

                    foreach (var o in entity.Opcoespergunta)
                        o.Correta = (o.Opcaoid == correta.Opcaoid);
                }
                else
                {
                    var qtdCorretas = entity.Opcoespergunta.Count(o => o.Correta);
                    if (qtdCorretas != 1)
                        return ServiceFail<PerguntaResponse>("Objetiva com gabarito deve ter exatamente 1 correta.");
                }

                await _ctx.SaveChangesAsync(ct);
            }
            else
            {
                foreach (var o in entity.Opcoespergunta) o.Correta = false;
                await _ctx.SaveChangesAsync(ct);
            }

            var resp = await MapPerguntaToResponse(entity, ct);
            return ServiceOk(resp);
        }

        public async Task<ServiceResult<PerguntaResponse>> CriarMultiplaAsync(
            CriarPerguntaMultiplaRequest req, CancellationToken ct = default)
        {
            if (req.Opcoes == null || req.Opcoes.Count < 2)
                return ServiceFail<PerguntaResponse>("Pergunta de múltipla escolha exige pelo menos 2 opções.");

            var entity = new PerguntaEntity
            {
                Pesquisaid = req.PesquisaId,
                Tipoperguntaid = TIPO_MULTIPLA,
                Texto = req.Texto,
                Temgabarito = req.TemGabarito,
                Permitemultiplaselecao = req.PermiteMultiplaSelecao,
                Ordem = req.Ordem
            };

            foreach (var o in req.Opcoes)
            {
                entity.Opcoespergunta.Add(new OpcaoEntity
                {
                    Pergunta = entity,
                    Texto = o.Texto,
                    Correta = o.Correta ?? false
                });
            }

            _ctx.Perguntas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            await _ctx.Entry(entity).Collection(p => p.Opcoespergunta).LoadAsync(ct);

            if (req.TemGabarito)
            {
                if (req.OpcoesCorretasIds != null && req.OpcoesCorretasIds.Count > 0)
                {
                    var setCorretas = req.OpcoesCorretasIds.ToHashSet();
                    foreach (var o in entity.Opcoespergunta)
                        o.Correta = setCorretas.Contains(o.Opcaoid);
                }
                await _ctx.SaveChangesAsync(ct);
            }
            else
            {
                foreach (var o in entity.Opcoespergunta) o.Correta = false;
                await _ctx.SaveChangesAsync(ct);
            }

            var resp = await MapPerguntaToResponse(entity, ct);
            return ServiceOk(resp);
        }

        // -------------------- UPDATE --------------------

        public async Task<ServiceResult<PerguntaResponse>> AtualizarDiscursivaAsync(
            AtualizarPerguntaDiscursivaRequest req, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == req.PerguntaId, ct);

            if (entity == null) return ServiceFail<PerguntaResponse>("Pergunta não encontrada.");
            if (entity.Tipoperguntaid != TIPO_DISCURSIVA) return ServiceFail<PerguntaResponse>("Tipo não é Discursiva.");

            entity.Texto = req.Texto;
            entity.Ordem = req.Ordem;
            entity.Temgabarito = false;
            entity.Permitemultiplaselecao = false;

            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(await MapPerguntaToResponse(entity, ct));
        }

        public async Task<ServiceResult<PerguntaResponse>> AtualizarObjetivaAsync(
            AtualizarPerguntaObjetivaRequest req, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == req.PerguntaId, ct);

            if (entity == null) return ServiceFail<PerguntaResponse>("Pergunta não encontrada.");
            if (entity.Tipoperguntaid != TIPO_OBJETIVA) return ServiceFail<PerguntaResponse>("Tipo não é Objetiva.");

            entity.Texto = req.Texto;
            entity.Ordem = req.Ordem;
            entity.Temgabarito = req.TemGabarito;
            entity.Permitemultiplaselecao = false;

            if (req.TemGabarito)
            {
                if (!req.OpcaoCorretaId.HasValue)
                    return ServiceFail<PerguntaResponse>("Para Objetiva com gabarito, informe OpcaoCorretaId.");

                var correta = entity.Opcoespergunta.FirstOrDefault(o => o.Opcaoid == req.OpcaoCorretaId.Value);
                if (correta == null)
                    return ServiceFail<PerguntaResponse>("OpcaoCorretaId inválida para esta pergunta.");

                foreach (var o in entity.Opcoespergunta)
                    o.Correta = (o.Opcaoid == correta.Opcaoid);
            }
            else
            {
                foreach (var o in entity.Opcoespergunta) o.Correta = false;
            }

            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(await MapPerguntaToResponse(entity, ct));
        }

        public async Task<ServiceResult<PerguntaResponse>> AtualizarMultiplaAsync(
            AtualizarPerguntaMultiplaRequest req, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == req.PerguntaId, ct);

            if (entity == null) return ServiceFail<PerguntaResponse>("Pergunta não encontrada.");
            if (entity.Tipoperguntaid != TIPO_MULTIPLA) return ServiceFail<PerguntaResponse>("Tipo não é Múltipla.");

            entity.Texto = req.Texto;
            entity.Ordem = req.Ordem;
            entity.Temgabarito = req.TemGabarito;
            entity.Permitemultiplaselecao = req.PermiteMultiplaSelecao;

            if (req.TemGabarito)
            {
                var corretas = (req.OpcoesCorretasIds ?? new List<int>()).ToHashSet();
                foreach (var o in entity.Opcoespergunta)
                    o.Correta = corretas.Contains(o.Opcaoid);
            }
            else
            {
                foreach (var o in entity.Opcoespergunta) o.Correta = false;
            }

            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(await MapPerguntaToResponse(entity, ct));
        }

        // -------------------- DELETE --------------------

        public async Task<ServiceResult<bool>> ExcluirAsync(int perguntaId, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas.FirstOrDefaultAsync(p => p.Perguntaid == perguntaId, ct);
            if (entity == null) return ServiceFail<bool>("Pergunta não encontrada.");

            _ctx.Perguntas.Remove(entity);
            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(true);
        }

        // -------------------- GET --------------------

        public async Task<ServiceResult<PerguntaResponse>> ObterPorIdAsync(int perguntaId, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == perguntaId, ct);

            if (entity == null) return ServiceFail<PerguntaResponse>("Pergunta não encontrada.");
            return ServiceOk(await MapPerguntaToResponse(entity, ct));
        }

        public async Task<ServiceResult<IReadOnlyList<PerguntaResponse>>> ListarPorPesquisaAsync(
            int pesquisaId, CancellationToken ct = default)
        {
            var list = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .Where(p => p.Pesquisaid == pesquisaId)
                .OrderBy(p => p.Ordem)
                .ToListAsync(ct);

            var resp = new List<PerguntaResponse>(list.Count);
            foreach (var p in list)
                resp.Add(await MapPerguntaToResponse(p, ct));

            return ServiceOk<IReadOnlyList<PerguntaResponse>>(resp);
        }

        // -------------------- GABARITO --------------------

        public async Task<ServiceResult<bool>> DefinirGabaritoAsync(
            int perguntaId, IEnumerable<int> opcaoIds, bool permitirApenasUma, CancellationToken ct = default)
        {
            var entity = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == perguntaId, ct);

            if (entity == null) return ServiceFail<bool>("Pergunta não encontrada.");

            var set = (opcaoIds ?? Enumerable.Empty<int>()).ToHashSet();

            if (permitirApenasUma)
            {
                if (set.Count != 1) return ServiceFail<bool>("Objetiva: deve haver exatamente 1 correta.");
                foreach (var o in entity.Opcoespergunta)
                    o.Correta = set.Contains(o.Opcaoid);
            }
            else
            {
                foreach (var o in entity.Opcoespergunta)
                    o.Correta = set.Contains(o.Opcaoid);
            }

            entity.Temgabarito = set.Count > 0;

            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(true);
        }

        // -------------------- ORDEM --------------------

        public async Task<ServiceResult<bool>> ReordenarAsync(
            int pesquisaId, IReadOnlyList<int> perguntaIdsNaOrdem, CancellationToken ct = default)
        {
            if (perguntaIdsNaOrdem == null || perguntaIdsNaOrdem.Count == 0)
                return ServiceFail<bool>("Lista de perguntas vazia.");

            var perguntas = await _ctx.Perguntas
                .Where(p => p.Pesquisaid == pesquisaId && perguntaIdsNaOrdem.Contains(p.Perguntaid))
                .ToListAsync(ct);

            var posById = perguntaIdsNaOrdem
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            foreach (var p in perguntas)
                if (posById.TryGetValue(p.Perguntaid, out var idx))
                    p.Ordem = idx;

            await _ctx.SaveChangesAsync(ct);
            return ServiceOk(true);
        }

        // -------------------- OPÇÕES --------------------

        public async Task<ServiceResult<OpcaoPerguntaResponse>> AdicionarOpcaoAsync(
            int perguntaId, OpcaoPerguntaRequest req, CancellationToken ct = default)
        {
            var pergunta = await _ctx.Perguntas
                .Include(p => p.Opcoespergunta)
                .FirstOrDefaultAsync(p => p.Perguntaid == perguntaId, ct);

            if (pergunta == null) return ServiceFail<OpcaoPerguntaResponse>("Pergunta não encontrada.");

            var opc = new OpcaoEntity
            {
                Perguntaid = perguntaId,
                Texto = req.Texto,
                Correta = req.Correta ?? false
            };

            pergunta.Opcoespergunta.Add(opc);
            await _ctx.SaveChangesAsync(ct);

            var resp = new OpcaoPerguntaResponse
            {
                OpcaoId = opc.Opcaoid,
                PerguntaId = opc.Perguntaid,
                Texto = opc.Texto,
                Ordem = 0,
                Ativa = true,
                Correta = opc.Correta
            };

            return ServiceOk(resp);
        }

        public async Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarOpcaoAsync(
            OpcaoPerguntaUpdateRequest req, CancellationToken ct = default)
        {
            var opc = await _ctx.Set<OpcaoEntity>()
                .FirstOrDefaultAsync(o => o.Opcaoid == req.OpcaoId, ct);

            if (opc == null) return ServiceFail<OpcaoPerguntaResponse>("Opção não encontrada.");

            if (req.Texto != null) opc.Texto = req.Texto;
            if (req.Correta.HasValue) opc.Correta = req.Correta.Value;

            await _ctx.SaveChangesAsync(ct);

            var resp = new OpcaoPerguntaResponse
            {
                OpcaoId = opc.Opcaoid,
                PerguntaId = opc.Perguntaid,
                Texto = opc.Texto,
                Ordem = 0,
                Ativa = true,
                Correta = opc.Correta
            };

            return ServiceOk(resp);
        }

        public async Task<ServiceResult<bool>> RemoverOpcaoAsync(
            int opcaoId, bool softDelete = true, CancellationToken ct = default)
        {
            var opc = await _ctx.Set<OpcaoEntity>().FirstOrDefaultAsync(o => o.Opcaoid == opcaoId, ct);
            if (opc == null) return ServiceFail<bool>("Opção não encontrada.");

            _ctx.Remove(opc);
            await _ctx.SaveChangesAsync(ct);

            return ServiceOk(true);
        }

        public Task<ServiceResult<bool>> ReordenarOpcoesAsync(
            int perguntaId, IReadOnlyList<int> opcaoIdsNaOrdem, CancellationToken ct = default)
        {
            return Task.FromResult(ServiceFail<bool>(
                "Reordenação de opções não suportada: o modelo 'Opcoespergunta' não possui coluna de ordenação."));
        }

        // -------------------- HELPERS --------------------

        private async Task<PerguntaResponse> MapPerguntaToResponse(PerguntaEntity p, CancellationToken ct)
        {
            if (_ctx.Entry(p).Collection(x => x.Opcoespergunta).IsLoaded == false)
                await _ctx.Entry(p).Collection(x => x.Opcoespergunta).LoadAsync(ct);

            var tipo = p.Tipoperguntaid switch
            {
                TIPO_DISCURSIVA => TipoPerguntaDto.Discursiva,
                TIPO_OBJETIVA => TipoPerguntaDto.Objetiva,
                TIPO_MULTIPLA => TipoPerguntaDto.MultiplaEscolha,
                _ => TipoPerguntaDto.Discursiva
            };

            var resp = new PerguntaResponse
            {
                PerguntaId = p.Perguntaid,
                PesquisaId = p.Pesquisaid,
                Tipo = tipo,
                Texto = p.Texto,
                Obrigatoria = false, // model não tem esse campo
                Ordem = p.Ordem,
                TemGabarito = p.Temgabarito,
                PermiteMultiplaSelecao = p.Permitemultiplaselecao,
                Opcoes = p.Opcoespergunta?
                    .OrderBy(o => o.Opcaoid)
                    .Select(o => new OpcaoPerguntaResponse
                    {
                        OpcaoId = o.Opcaoid,
                        PerguntaId = o.Perguntaid,
                        Texto = o.Texto,
                        Ordem = 0,
                        Ativa = true,
                        Correta = o.Correta
                    })
                    .ToList()
            };

            if (tipo == TipoPerguntaDto.Objetiva)
            {
                var correta = p.Opcoespergunta?.FirstOrDefault(o => o.Correta);
                resp.OpcaoCorretaId = correta?.Opcaoid;
            }
            else if (tipo == TipoPerguntaDto.MultiplaEscolha)
            {
                resp.OpcoesCorretasIds = p.Opcoespergunta?.Where(o => o.Correta).Select(o => o.Opcaoid).ToList();
            }

            return resp;
        }

        private ServiceResult<T> ServiceOk<T>(T data) => ServiceResult<T>.Ok(data);
        private ServiceResult<T> ServiceFail<T>(string message) => ServiceResult<T>.Fail("Error", message);
    }
}
