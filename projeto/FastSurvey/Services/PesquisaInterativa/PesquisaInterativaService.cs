#nullable enable
using System.Security.Cryptography;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public sealed class PesquisaInterativaService : IPesquisaInterativaService
    {
        private readonly FastSurveyContext _ctx;

        public PesquisaInterativaService(FastSurveyContext ctx) => _ctx = ctx;

        // =========================
        // INICIAR
        // =========================
        public async Task<ServiceResult<SessaoInterativaResponse>> IniciarSessaoAsync(
            PesquisaInterativaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var pesquisa = await _ctx
                    .Pesquisas.Include(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PesquisaId == request.PesquisaId, ct);

                if (pesquisa is null)
                    return ServiceResult<SessaoInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Pesquisa não encontrada"
                    );

                if (!pesquisa.IsInterativa)
                    return ServiceResult<SessaoInterativaResponse>.Fail(
                        "NOT_INTERACTIVE",
                        "Esta pesquisa não é interativa"
                    );

                // Uma sessão por pesquisa
                var sessaoExistente = await _ctx.Set<SessoesInterativas>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.PesquisaId == request.PesquisaId && s.Ativa, ct);

                if (sessaoExistente != null)
                    return ServiceResult<SessaoInterativaResponse>.Fail(
                        "ACTIVE_SESSION",
                        $"Já existe uma sessão ativa. Código: {sessaoExistente.CodigoAcesso}"
                    );

                // Gera código único
                var codigo = await GerarCodigoUnicoAsync(ct);

                var agora = DateTime.UtcNow;
                var sessao = new SessoesInterativas
                {
                    SessaoId = Guid.NewGuid().ToString("N"),
                    PesquisaId = pesquisa.PesquisaId,
                    CodigoAcesso = codigo,
                    CriadaEm = agora,
                    IniciadaEm = request.IsAtiva ? agora : null,
                    Ativa = true,
                };

                _ctx.Add(sessao);
                await _ctx.SaveChangesAsync(ct);

                var resp = new SessaoInterativaResponse
                {
                    SessaoId = sessao.SessaoId,
                    CodigoAcesso = sessao.CodigoAcesso,
                    PesquisaId = pesquisa.PesquisaId,
                    TituloPesquisa = pesquisa.Titulo ?? $"Pesquisa {pesquisa.PesquisaId}",
                    Ativa = sessao.Ativa,
                    CriadaEm = sessao.CriadaEm,
                    IniciadaEm = sessao.IniciadaEm,
                    TotalParticipantes = 0,
                    Perguntas = pesquisa
                        .Perguntas.OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.PerguntaId,
                            Texto = p.Texto,
                            Tipo = p.TipoPerguntaId,
                            Ordem = p.Ordem,
                            Opcoes = (p.OpcoesPergunta ?? new List<OpcoesPergunta>())
                                .OrderBy(o => o.Ordem)
                                .Select(o => new OpcaoInterativaResponse
                                {
                                    OpcaoId = o.OpcaoId,
                                    Texto = o.Texto,
                                    Ordem = o.Ordem,
                                })
                                .ToList(),
                        })
                        .ToList(),
                };

                return ServiceResult<SessaoInterativaResponse>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<SessaoInterativaResponse>.Fail(
                    "ERROR",
                    $"Erro ao iniciar sessão: {ex.Message}"
                );
            }
        }

        // =========================
        // ENTRAR
        // =========================
        public async Task<ServiceResult<ParticipanteResponse>> EntrarSessaoAsync(
            EntrarSessaoRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(
                        s => s.CodigoAcesso == request.CodigoAcesso && s.Ativa,
                        ct
                    );

                if (sessao is null)
                    return ServiceResult<ParticipanteResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                var participante = new SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao
                {
                    SessaoId = sessao.SessaoId,
                    NomeParticipante = string.IsNullOrWhiteSpace(request.NomeParticipante)
                        ? "Participante"
                        : request.NomeParticipante,
                    EntrouEm = DateTime.UtcNow,
                };

                _ctx.Add(participante);
                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<ParticipanteResponse>.Ok(
                    new ParticipanteResponse
                    {
                        ParticipanteId = participante.ParticipanteId,
                        Nome = participante.NomeParticipante,
                        SessaoId = sessao.SessaoId,
                        EntrouEm = participante.EntrouEm,
                        Ativo = true,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<ParticipanteResponse>.Fail(
                    "ERROR",
                    $"Erro ao entrar na sessão: {ex.Message}"
                );
            }
        }

        // =========================
        // OBTER SESSÃO
        // =========================
        public async Task<ServiceResult<SessaoInterativaResponse>> ObterSessaoAsync(
            string codigo,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.CodigoAcesso == codigo, ct);

                if (sessao is null)
                    return ServiceResult<SessaoInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                var resp = new SessaoInterativaResponse
                {
                    SessaoId = sessao.SessaoId,
                    CodigoAcesso = sessao.CodigoAcesso,
                    PesquisaId = sessao.PesquisaId,
                    TituloPesquisa = sessao.Pesquisa?.Titulo ?? $"Pesquisa {sessao.PesquisaId}",
                    Ativa = sessao.Ativa,
                    CriadaEm = sessao.CriadaEm,
                    IniciadaEm = sessao.IniciadaEm,
                    FinalizadaEm = sessao.FinalizadaEm,
                    TotalParticipantes = sessao.ParticipantesSessao.Count,
                    Perguntas = (sessao.Pesquisa?.Perguntas ?? new List<Perguntas>())
                        .OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.PerguntaId,
                            Texto = p.Texto,
                            Tipo = p.TipoPerguntaId,
                            Ordem = p.Ordem,
                            Opcoes = (p.OpcoesPergunta ?? new List<OpcoesPergunta>())
                                .OrderBy(o => o.Ordem)
                                .Select(o => new OpcaoInterativaResponse
                                {
                                    OpcaoId = o.OpcaoId,
                                    Texto = o.Texto,
                                    Ordem = o.Ordem,
                                })
                                .ToList(),
                        })
                        .ToList(),
                };

                return ServiceResult<SessaoInterativaResponse>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<SessaoInterativaResponse>.Fail(
                    "ERROR",
                    $"Erro ao obter sessão: {ex.Message}"
                );
            }
        }

        // =========================
        // RESPONDER
        // =========================
        public async Task<ServiceResult<RespostaInterativaResponse>> ResponderPerguntaAsync(
            RespostaInterativaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                // Sessão precisa existir e estar ativa
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                var pergunta = await _ctx
                    .Perguntas.Include(p => p.OpcoesPergunta)
                    .FirstOrDefaultAsync(p => p.PerguntaId == request.PerguntaId, ct);

                if (pergunta is null)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Pergunta não encontrada"
                    );

                // Validações de tipo x payload
                var opcoesSelecionadas =
                    request.OpcoesSelecionadas?.Distinct().ToList() ?? new List<int>();

                if (pergunta.TipoPerguntaId == 1) // Discursiva
                {
                    if (string.IsNullOrWhiteSpace(request.TextoResposta))
                        return ServiceResult<RespostaInterativaResponse>.Fail(
                            "VALIDATION_ERROR",
                            "Resposta discursiva deve conter texto."
                        );
                    opcoesSelecionadas.Clear();
                }
                else
                {
                    // Objetiva/Multipla: pelo menos uma opção
                    if (opcoesSelecionadas.Count == 0)
                        return ServiceResult<RespostaInterativaResponse>.Fail(
                            "VALIDATION_ERROR",
                            "Selecione ao menos uma opção."
                        );

                    // Todas as opções selecionadas devem pertencer à pergunta
                    var opcoesValidas = (pergunta.OpcoesPergunta ?? new List<OpcoesPergunta>())
                        .Select(o => o.OpcaoId)
                        .ToHashSet();
                    if (!opcoesSelecionadas.All(id => opcoesValidas.Contains(id)))
                        return ServiceResult<RespostaInterativaResponse>.Fail(
                            "VALIDATION_ERROR",
                            "Há opção inválida para esta pergunta."
                        );
                }

                // Persistência (transação simples)
                using var tx = await _ctx.Database.BeginTransactionAsync(ct);

                var resposta = new Respostas
                {
                    PerguntaId = pergunta.PerguntaId,
                    Texto = request.TextoResposta ?? string.Empty,
                    DataResposta = DateTime.UtcNow,
                    SessaoId = request.SessaoId,
                    RespostaAnonima = true,
                };

                _ctx.Respostas.Add(resposta);
                await _ctx.SaveChangesAsync(ct);

                if (opcoesSelecionadas.Count > 0)
                {
                    var opcoes = await _ctx.Set<OpcoesPergunta>()
                        .Where(o => opcoesSelecionadas.Contains(o.OpcaoId))
                        .ToListAsync(ct);

                    foreach (var opc in opcoes)
                        resposta.Opcao.Add(opc);

                    await _ctx.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);

                // resultados “tempo real”
                var resultados = await ObterResultadosTempoRealAsync(request.SessaoId, ct);

                return ServiceResult<RespostaInterativaResponse>.Ok(
                    new RespostaInterativaResponse
                    {
                        Sucesso = true,
                        Mensagem = "Resposta registrada com sucesso",
                        RespostaId = resposta.RespostaId,
                        ResultadosTempoReal = resultados.Success ? resultados.Data : null,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<RespostaInterativaResponse>.Fail(
                    "ERROR",
                    $"Erro ao responder pergunta: {ex.Message}"
                );
            }
        }

        // =========================
        // RESULTADOS EM TEMPO REAL (1 query agregada)
        // =========================
        public async Task<ServiceResult<object>> ObterResultadosTempoRealAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            try
            {
                // Carrega perguntas e opções da sessão (somente o necessário)
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessaoId == sessaoId, ct);

                if (sessao is null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Sessão não encontrada");

                var perguntas = (sessao.Pesquisa?.Perguntas ?? new List<Perguntas>())
                    .OrderBy(p => p.Ordem)
                    .ToList();
                if (perguntas.Count == 0)
                    return ServiceResult<object>.Ok(
                        new
                        {
                            sessaoId,
                            totalParticipantes = 0,
                            perguntas = Array.Empty<object>(),
                        }
                    );

                var perguntaIds = perguntas.Select(p => p.PerguntaId).ToList();

                // 1) total de respostas por pergunta na sessão
                var totaisPorPergunta = await _ctx
                    .Respostas.Where(r =>
                        r.SessaoId == sessaoId && perguntaIds.Contains(r.PerguntaId)
                    )
                    .GroupBy(r => r.PerguntaId)
                    .Select(g => new { PerguntaId = g.Key, Total = g.Count() })
                    .ToDictionaryAsync(x => x.PerguntaId, x => x.Total, ct);

                // 2) contagem por opção (join na many-to-many Resposta.Opcao)
                var totaisPorOpcao = await _ctx
                    .Respostas.Where(r =>
                        r.SessaoId == sessaoId && perguntaIds.Contains(r.PerguntaId)
                    )
                    .SelectMany(r => r.Opcao.Select(o => new { r.PerguntaId, o.OpcaoId }))
                    .GroupBy(x => new { x.PerguntaId, x.OpcaoId })
                    .Select(g => new
                    {
                        g.Key.PerguntaId,
                        g.Key.OpcaoId,
                        Total = g.Count(),
                    })
                    .ToListAsync(ct);

                var mapaOpcao = totaisPorOpcao
                    .GroupBy(x => x.PerguntaId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(k => k.OpcaoId, v => v.Total));

                // 3) participantes ativos
                var totalParticipantes = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao>()
                    .AsNoTracking()
                    .CountAsync(p => p.SessaoId == sessaoId && p.SaiuEm == null, ct);

                // 4) montar resposta
                var resultado = new
                {
                    sessaoId,
                    totalParticipantes,
                    perguntas = perguntas
                        .Select(p =>
                        {
                            var totalResp = totaisPorPergunta.GetValueOrDefault(p.PerguntaId);
                            var dictOpc = mapaOpcao.GetValueOrDefault(
                                p.PerguntaId,
                                new Dictionary<int, int>()
                            );

                            var opcoes = (p.OpcoesPergunta ?? new List<OpcoesPergunta>())
                                .OrderBy(o => o.Ordem)
                                .Select(o => new
                                {
                                    opcaoId = o.OpcaoId,
                                    texto = o.Texto,
                                    totalRespostas = dictOpc.GetValueOrDefault(o.OpcaoId),
                                })
                                .ToList();

                            return new
                            {
                                perguntaId = p.PerguntaId,
                                texto = p.Texto,
                                totalRespostas = totalResp,
                                opcoes,
                            };
                        })
                        .ToList(),
                };

                return ServiceResult<object>.Ok(resultado);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter resultados: {ex.Message}"
                );
            }
        }

        // =========================
        // FINALIZAR
        // =========================
        public async Task<ServiceResult<bool>> FinalizarSessaoAsync(
            FinalizarSessaoRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId, ct);

                if (sessao is null)
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Sessão não encontrada");

                if (!sessao.Ativa)
                    return ServiceResult<bool>.Ok(true); // idempotente

                sessao.FinalizadaEm = DateTime.UtcNow;
                sessao.Ativa = false;

                var participantes = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao>()
                    .Where(p => p.SessaoId == request.SessaoId && p.SaiuEm == null)
                    .ToListAsync(ct);

                foreach (var p in participantes)
                    p.SaiuEm = DateTime.UtcNow;

                await _ctx.SaveChangesAsync(ct);
                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao finalizar sessão: {ex.Message}");
            }
        }

        // =========================
        // STATUS
        // =========================
        public async Task<ServiceResult<object>> ObterStatusSessaoAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessaoId == sessaoId, ct);

                if (sessao is null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Sessão não encontrada");

                return ServiceResult<object>.Ok(
                    new
                    {
                        sessaoId = sessao.SessaoId,
                        ativa = sessao.Ativa,
                        totalParticipantes = sessao.ParticipantesSessao.Count,
                        criadaEm = sessao.CriadaEm,
                        iniciadaEm = sessao.IniciadaEm,
                        finalizadaEm = sessao.FinalizadaEm,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter status da sessão: {ex.Message}"
                );
            }
        }

        // =========================
        // PARTICIPANTES
        // =========================
        public async Task<ServiceResult<List<ParticipanteResponse>>> ObterParticipantesAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            try
            {
                var participantes = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao>()
                    .Where(p => p.SessaoId == sessaoId)
                    .OrderBy(p => p.EntrouEm)
                    .AsNoTracking()
                    .ToListAsync(ct);

                var resp = participantes
                    .Select(p => new ParticipanteResponse
                    {
                        ParticipanteId = p.ParticipanteId,
                        Nome = p.NomeParticipante,
                        SessaoId = p.SessaoId,
                        EntrouEm = p.EntrouEm,
                        Ativo = p.SaiuEm == null,
                    })
                    .ToList();

                return ServiceResult<List<ParticipanteResponse>>.Ok(resp);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ParticipanteResponse>>.Fail(
                    "ERROR",
                    $"Erro ao obter participantes: {ex.Message}"
                );
            }
        }

        // =========================
        // Helpers
        // =========================
        private async Task<string> GerarCodigoUnicoAsync(CancellationToken ct)
        {
            const string charset = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem confusos: I, O, 0, 1
            const int tamanho = 6;

            for (var tentativas = 0; tentativas < 20; tentativas++)
            {
                var codigo = string.Create(
                    tamanho,
                    (object?)null,
                    (span, _) =>
                    {
                        for (int i = 0; i < span.Length; i++)
                        {
                            var idx = RandomNumberGenerator.GetInt32(charset.Length);
                            span[i] = charset[idx];
                        }
                    }
                );

                var existe = await _ctx.Set<SessoesInterativas>()
                    .AnyAsync(s => s.CodigoAcesso == codigo && s.Ativa, ct);
                if (!existe)
                    return codigo;
            }

            // fallback improvável
            return Guid.NewGuid().ToString("N")[..tamanho].ToUpperInvariant();
        }
    }
}
