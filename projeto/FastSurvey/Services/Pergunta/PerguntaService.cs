// FASTSURVEY/Services/Pergunta/PerguntaService.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes; // OpcaoPerguntaRequest, OpcaoPerguntaUpdateRequest, OpcaoPerguntaResponse
using FASTSURVEY.Dtos.Perguntas; // CriarPerguntaRequest, AtualizarPerguntaRequest, PerguntaResponse, TipoPerguntaDto
using FASTSURVEY.Services.Result; // ServiceResult<T>
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using OpcaoEntity = SISTEMA_FASTSURVEY.MODEL.Models.OpcoesPergunta;
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

        // =============== CREATE ===============
        public async Task<ServiceResult<PerguntaResponse>> CriarAsync(
            CriarPerguntaRequest req,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(req.Texto))
                return Fail<PerguntaResponse>("Texto da pergunta é obrigatório.");

            var tipoId = req.Tipo switch
            {
                TipoPerguntaDto.Discursiva => TIPO_DISCURSIVA,
                TipoPerguntaDto.Objetiva => TIPO_OBJETIVA,
                TipoPerguntaDto.MultiplaEscolha => TIPO_MULTIPLA,
                _ => TIPO_DISCURSIVA,
            };

            // Validações por tipo
            if (req.Tipo == TipoPerguntaDto.Objetiva)
            {
                if (req.Opcoes is null || req.Opcoes.Count < 2)
                    return Fail<PerguntaResponse>("Objetiva exige ao menos 2 opções.");
                req.PermiteMultiplasSelecao = false;
                if (req.TemGabarito && !req.OpcaoCorretaId.HasValue)
                    return Fail<PerguntaResponse>("Objetiva com gabarito requer OpcaoCorretaId.");
            }
            else if (req.Tipo == TipoPerguntaDto.MultiplaEscolha)
            {
                if (req.Opcoes is null || req.Opcoes.Count < 2)
                    return Fail<PerguntaResponse>("Múltipla exige ao menos 2 opções.");
                if (
                    req.TemGabarito
                    && (req.OpcoesCorretasIds is null || req.OpcoesCorretasIds.Count == 0)
                )
                    return Fail<PerguntaResponse>(
                        "Múltipla com gabarito requer ao menos 1 opção correta."
                    );
            }
            else // Discursiva
            {
                req.Opcoes = null;
                req.TemGabarito = false;
                req.PermiteMultiplasSelecao = false;
                req.OpcaoCorretaId = null;
                req.OpcoesCorretasIds = null;
            }

            var entity = new PerguntaEntity
            {
                PesquisaId = req.PesquisaId,
                TipoPerguntaId = tipoId,
                Texto = req.Texto,
                TemGabarito = req.TemGabarito,
                PermiteMultiplasSelecao = req.PermiteMultiplasSelecao ?? false,
                Ordem = req.Ordem,
            };

            _ctx.Perguntas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            // Opções (se houver)
            if (req.Opcoes != null && req.Opcoes.Count > 0)
            {
                foreach (var o in req.Opcoes)
                {
                    entity.OpcoesPergunta.Add(
                        new OpcaoEntity
                        {
                            PerguntaId = entity.PerguntaId,
                            Texto = o.Texto,
                            Ordem = o.Ordem,
                            Correta = o.Correta ?? false,
                            Ativa = true,
                        }
                    );
                }
                await _ctx.SaveChangesAsync(ct);

                await _ctx.Entry(entity).Collection(p => p.OpcoesPergunta).LoadAsync(ct);

                if (
                    req.Tipo == TipoPerguntaDto.Objetiva
                    && req.TemGabarito
                    && req.OpcaoCorretaId.HasValue
                )
                {
                    foreach (var o in entity.OpcoesPergunta)
                        o.Correta = false;
                    var correta = entity.OpcoesPergunta.FirstOrDefault(x =>
                        x.OpcaoId == req.OpcaoCorretaId.Value
                    );
                    if (correta != null)
                        correta.Correta = true;
                }
                else if (req.Tipo == TipoPerguntaDto.MultiplaEscolha)
                {
                    var set = (req.OpcoesCorretasIds ?? new List<int>()).ToHashSet();
                    foreach (var o in entity.OpcoesPergunta)
                        o.Correta = set.Contains(o.OpcaoId);
                }

                await _ctx.SaveChangesAsync(ct);
            }

            var resp = await MapPerguntaToResponse(entity, ct);
            return Ok(resp);
        }

        // =============== UPDATE ===============
        public async Task<ServiceResult<PerguntaResponse>> AtualizarAsync(
            AtualizarPerguntaRequest req,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PerguntaId == req.PerguntaId, ct);

            if (entity == null)
                return Fail<PerguntaResponse>("Pergunta não encontrada.");

            var tipoAtual = entity.TipoPerguntaId;
            var tipoReq = req.Tipo switch
            {
                TipoPerguntaDto.Discursiva => TIPO_DISCURSIVA,
                TipoPerguntaDto.Objetiva => TIPO_OBJETIVA,
                TipoPerguntaDto.MultiplaEscolha => TIPO_MULTIPLA,
                _ => tipoAtual,
            };
            if (tipoReq != tipoAtual)
                return Fail<PerguntaResponse>(
                    "Alterar o tipo de pergunta não é permitido. Exclua e crie novamente."
                );

            entity.Texto = req.Texto;
            entity.Ordem = req.Ordem;
            entity.TemGabarito = req.TemGabarito;
            entity.PermiteMultiplasSelecao =
                req.PermiteMultiplasSelecao ?? entity.PermiteMultiplasSelecao;

            if (tipoAtual == TIPO_DISCURSIVA)
            {
                entity.TemGabarito = false;
                entity.PermiteMultiplasSelecao = false;
                foreach (var o in entity.OpcoesPergunta)
                    o.Correta = false;
            }
            else if (tipoAtual == TIPO_OBJETIVA)
            {
                entity.PermiteMultiplasSelecao = false;
                if (req.TemGabarito)
                {
                    foreach (var o in entity.OpcoesPergunta)
                        o.Correta = false;
                    if (req.OpcaoCorretaId.HasValue)
                    {
                        var correta = entity.OpcoesPergunta.FirstOrDefault(o =>
                            o.OpcaoId == req.OpcaoCorretaId.Value
                        );
                        if (correta != null)
                            correta.Correta = true;
                    }
                }
                else
                {
                    foreach (var o in entity.OpcoesPergunta)
                        o.Correta = false;
                }
            }
            else // Múltipla
            {
                var set = (req.OpcoesCorretasIds ?? new List<int>()).ToHashSet();
                foreach (var o in entity.OpcoesPergunta)
                    o.Correta = req.TemGabarito && set.Contains(o.OpcaoId);
            }

            await _ctx.SaveChangesAsync(ct);
            var resp = await MapPerguntaToResponse(entity, ct);
            return Ok(resp);
        }

        // =============== DELETE ===============
        public async Task<ServiceResult<bool>> ExcluirAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx.Perguntas.FindAsync(perguntaId, ct);
            if (entity == null)
                return Fail<bool>("Pergunta não encontrada.");

            _ctx.Perguntas.Remove(entity);
            await _ctx.SaveChangesAsync(ct);
            return Ok(true);
        }

        // =============== READ ===============
        public async Task<ServiceResult<PerguntaResponse>> ObterPorIdAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PerguntaId == perguntaId, ct);

            if (entity == null)
                return Fail<PerguntaResponse>("Pergunta não encontrada.");
            return Ok(await MapPerguntaToResponse(entity, ct));
        }

        public async Task<ServiceResult<IReadOnlyList<PerguntaResponse>>> ListarPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var entities = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .Where(p => p.PesquisaId == pesquisaId)
                .OrderBy(p => p.Ordem)
                .ThenBy(p => p.PerguntaId)
                .ToListAsync(ct);

            var list = new List<PerguntaResponse>(entities.Count);
            foreach (var e in entities)
                list.Add(await MapPerguntaToResponse(e, ct));

            return Ok<IReadOnlyList<PerguntaResponse>>(list);
        }

        // =============== GABARITO ===============
        public async Task<ServiceResult<bool>> DefinirGabaritoAsync(
            int perguntaId,
            IEnumerable<int> opcaoIds,
            bool permitirApenasUma,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PerguntaId == perguntaId, ct);
            if (entity == null)
                return Fail<bool>("Pergunta não encontrada.");

            var set = (opcaoIds ?? Enumerable.Empty<int>()).ToHashSet();

            if (permitirApenasUma)
            {
                if (set.Count != 1)
                    return Fail<bool>("Objetiva: deve haver exatamente 1 correta.");
                foreach (var o in entity.OpcoesPergunta)
                    o.Correta = set.Contains(o.OpcaoId);
            }
            else
            {
                foreach (var o in entity.OpcoesPergunta)
                    o.Correta = set.Contains(o.OpcaoId);
            }

            entity.TemGabarito = set.Count > 0;
            await _ctx.SaveChangesAsync(ct);
            return Ok(true);
        }

        // =============== REORDENAR ===============
        public async Task<ServiceResult<bool>> ReordenarAsync(
            int pesquisaId,
            IReadOnlyList<int> perguntaIdsNaOrdem,
            CancellationToken ct = default
        )
        {
            var perguntas = await _ctx
                .Perguntas.Where(p =>
                    p.PesquisaId == pesquisaId && perguntaIdsNaOrdem.Contains(p.PerguntaId)
                )
                .ToListAsync(ct);

            if (perguntas.Count != perguntaIdsNaOrdem.Count)
                return Fail<bool>("Algumas perguntas não foram encontradas.");

            for (int i = 0; i < perguntaIdsNaOrdem.Count; i++)
            {
                var pergunta = perguntas.First(p => p.PerguntaId == perguntaIdsNaOrdem[i]);
                pergunta.Ordem = i + 1;
            }

            await _ctx.SaveChangesAsync(ct);
            return Ok(true);
        }

        // =============== OPÇÕES ===============
        public async Task<ServiceResult<OpcaoPerguntaResponse>> AdicionarOpcaoAsync(
            int perguntaId,
            OpcaoPerguntaRequest req,
            CancellationToken ct = default
        )
        {
            var pergunta = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PerguntaId == perguntaId, ct);
            if (pergunta == null)
                return Fail<OpcaoPerguntaResponse>("Pergunta não encontrada.");

            var opc = new OpcaoEntity
            {
                PerguntaId = perguntaId,
                Texto = req.Texto,
                Correta = req.Correta ?? false,
                Ordem = req.Ordem,
            };

            pergunta.OpcoesPergunta.Add(opc);
            await _ctx.SaveChangesAsync(ct);

            var resp = new OpcaoPerguntaResponse
            {
                OpcaoId = opc.OpcaoId,
                PerguntaId = opc.PerguntaId,
                Texto = opc.Texto,
                Ordem = opc.Ordem,
                Ativa = true,
                Correta = opc.Correta,
            };

            return Ok(resp);
        }

        public async Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarOpcaoAsync(
            OpcaoPerguntaUpdateRequest req,
            CancellationToken ct = default
        )
        {
            var opc = await _ctx.Set<OpcaoEntity>()
                .FirstOrDefaultAsync(o => o.OpcaoId == req.OpcaoId, ct);
            if (opc == null)
                return Fail<OpcaoPerguntaResponse>("Opção não encontrada.");

            if (req.Texto != null)
                opc.Texto = req.Texto;
            if (req.Correta.HasValue)
                opc.Correta = req.Correta.Value;
            if (req.Ordem.HasValue)
                opc.Ordem = req.Ordem.Value;

            await _ctx.SaveChangesAsync(ct);

            var resp = new OpcaoPerguntaResponse
            {
                OpcaoId = opc.OpcaoId,
                PerguntaId = opc.PerguntaId,
                Texto = opc.Texto,
                Ordem = opc.Ordem,
                Ativa = true,
                Correta = opc.Correta,
            };

            return Ok(resp);
        }

        public async Task<ServiceResult<bool>> RemoverOpcaoAsync(
            int opcaoId,
            bool softDelete = true,
            CancellationToken ct = default
        )
        {
            var opc = await _ctx.Set<OpcaoEntity>()
                .FirstOrDefaultAsync(o => o.OpcaoId == opcaoId, ct);
            if (opc == null)
                return Fail<bool>("Opção não encontrada.");

            _ctx.Remove(opc);
            await _ctx.SaveChangesAsync(ct);
            return Ok(true);
        }

        public async Task<ServiceResult<bool>> ReordenarOpcoesAsync(
            int perguntaId,
            IReadOnlyList<int> opcaoIdsNaOrdem,
            CancellationToken ct = default
        )
        {
            var pergunta = await _ctx
                .Perguntas.Include(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PerguntaId == perguntaId, ct);
            if (pergunta == null)
                return Fail<bool>("Pergunta não encontrada.");

            if (pergunta.OpcoesPergunta.Count != opcaoIdsNaOrdem.Count)
                return Fail<bool>("Algumas opções não foram encontradas.");

            for (int i = 0; i < opcaoIdsNaOrdem.Count; i++)
            {
                var opcao = pergunta.OpcoesPergunta.First(o => o.OpcaoId == opcaoIdsNaOrdem[i]);
                opcao.Ordem = i + 1;
            }

            await _ctx.SaveChangesAsync(ct);
            return Ok(true);
        }

        // =============== Helpers ===============
        private async Task<PerguntaResponse> MapPerguntaToResponse(
            PerguntaEntity p,
            CancellationToken ct
        )
        {
            if (!_ctx.Entry(p).Collection(x => x.OpcoesPergunta).IsLoaded)
                await _ctx.Entry(p).Collection(x => x.OpcoesPergunta).LoadAsync(ct);

            var tipo = p.TipoPerguntaId switch
            {
                TIPO_DISCURSIVA => TipoPerguntaDto.Discursiva,
                TIPO_OBJETIVA => TipoPerguntaDto.Objetiva,
                TIPO_MULTIPLA => TipoPerguntaDto.MultiplaEscolha,
                _ => TipoPerguntaDto.Discursiva,
            };

            return new PerguntaResponse
            {
                PerguntaId = p.PerguntaId,
                PesquisaId = p.PesquisaId,
                Tipo = tipo,
                Texto = p.Texto,
                TemGabarito = p.TemGabarito,
                PermiteMultiplasSelecao = p.PermiteMultiplasSelecao,
                Ordem = p.Ordem,
                Opcoes = p
                    .OpcoesPergunta.OrderBy(o => o.Ordem)
                    .ThenBy(o => o.OpcaoId)
                    .Select(o => new OpcaoPerguntaResponse
                    {
                        OpcaoId = o.OpcaoId,
                        PerguntaId = o.PerguntaId,
                        Texto = o.Texto ?? string.Empty,
                        Ordem = o.Ordem,
                        Ativa = o.Ativa,
                        Correta = o.Correta,
                    })
                    .ToList(),
            };
        }

        private static ServiceResult<T> Ok<T>(T data) => ServiceResult<T>.Ok(data);

        private static ServiceResult<T> Fail<T>(string m) => ServiceResult<T>.Fail("ERROR", m);
    }
}
