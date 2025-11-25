#nullable enable
using System.Security.Cryptography;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Cache;
using FASTSURVEY.Services.Result;
using FASTSURVEY.Services.Resultados;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public sealed class PesquisaInterativaService : IPesquisaInterativaService
    {
        private readonly FastSurveyContext _ctx;
        private readonly ICacheService _cache;

        public PesquisaInterativaService(FastSurveyContext ctx, ICacheService cache)
        {
            _ctx = ctx;
            _cache = cache;
        }

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
                    ModoApresentacao = true,
                    PerguntaAtiva = false,
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
                    PerguntaAtualId = sessao.PerguntaAtualId,
                    OrdemPerguntaAtual = sessao.OrdemPerguntaAtual,
                    ModoApresentacao = sessao.ModoApresentacao,
                    PerguntaAtiva = sessao.PerguntaAtiva,
                    Perguntas = pesquisa
                        .Perguntas.OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.PerguntaId,
                            Texto = p.Texto,
                            Tipo = p.TipoPerguntaId.ToString(),
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
                    Ativo = true,
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
                    TotalParticipantes = sessao.ParticipantesSessao.Count,
                    PerguntaAtualId = sessao.PerguntaAtualId,
                    OrdemPerguntaAtual = sessao.OrdemPerguntaAtual,
                    ModoApresentacao = sessao.ModoApresentacao,
                    PerguntaAtiva = sessao.PerguntaAtiva,
                    Perguntas = sessao.Pesquisa?.Perguntas
                        .OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.PerguntaId,
                            Texto = p.Texto,
                            Tipo = p.TipoPerguntaId.ToString(),
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
                        .ToList() ?? new List<PerguntaInterativaResponse>(),
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
        // RESPONDER PERGUNTA
        // =========================
        public async Task<ServiceResult<RespostaInterativaResponse>> ResponderPerguntaAsync(
            RespostaInterativaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                if (!sessao.PerguntaAtiva)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "QUESTION_NOT_ACTIVE",
                        "A pergunta atual não está ativa para respostas"
                    );

                if (sessao.PerguntaAtualId != request.PerguntaId)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "WRONG_QUESTION",
                        "A pergunta respondida não é a pergunta atual"
                    );

                var participante = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao>()
                    .FirstOrDefaultAsync(p => p.ParticipanteId == request.ParticipanteId && p.Ativo, ct);

                if (participante is null)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "NOT_FOUND",
                        "Participante não encontrado"
                    );

                // Verificar se já respondeu esta pergunta
                var respostaExistente = await _ctx.Set<Respostas>()
                    .FirstOrDefaultAsync(r => r.ParticipanteId == request.ParticipanteId && r.PerguntaId == request.PerguntaId, ct);

                if (respostaExistente != null)
                    return ServiceResult<RespostaInterativaResponse>.Fail(
                        "ALREADY_ANSWERED",
                        "Você já respondeu esta pergunta"
                    );

                var resposta = new Respostas
                {
                    ParticipanteId = request.ParticipanteId,
                    PerguntaId = request.PerguntaId,
                    Texto = request.TextoResposta?.Trim() ?? string.Empty,
                    TextoResposta = request.TextoResposta,
                    DataResposta = DateTime.UtcNow,
                    RespondidaEm = DateTime.UtcNow,
                };

                _ctx.Add(resposta);

                // Adicionar opções selecionadas se houver
                if (request.OpcoesSelecionadas.Any())
                {
                    var opcoes = await _ctx.Set<OpcoesPergunta>()
                        .Where(o => request.OpcoesSelecionadas.Contains(o.OpcaoId))
                        .ToListAsync(ct);

                    foreach (var opcao in opcoes)
                    {
                        resposta.Opcao.Add(opcao);
                    }
                }

                await _ctx.SaveChangesAsync(ct);
                await ResultadosCacheHelper.InvalidatePesquisaAsync(
                    _cache,
                    sessao.PesquisaId,
                    new[] { request.PerguntaId },
                    ct
                );

                return ServiceResult<RespostaInterativaResponse>.Ok(
                    new RespostaInterativaResponse
                    {
                        Sucesso = true,
                        Mensagem = "Resposta registrada com sucesso",
                        RespostaId = resposta.RespostaId,
                    }
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<RespostaInterativaResponse>.Fail(
                    "ERROR",
                    $"Erro ao registrar resposta: {ex.Message}"
                );
            }
        }

        // =========================
        // AVANÇAR PERGUNTA
        // =========================
        public async Task<ServiceResult<PerguntaAtualResponse>> AvancarPerguntaAsync(
            AvancarPerguntaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                var perguntas = sessao.Pesquisa?.Perguntas.OrderBy(p => p.Ordem).ToList() ?? new List<Perguntas>();
                var totalPerguntas = perguntas.Count;

                if (totalPerguntas == 0)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NO_QUESTIONS",
                        "A pesquisa não possui perguntas"
                    );

                int? proximaOrdem;
                if (request.PerguntaId.HasValue)
                {
                    // Ir para pergunta específica
                    var perguntaEspecifica = perguntas.FirstOrDefault(p => p.PerguntaId == request.PerguntaId.Value);
                    if (perguntaEspecifica == null)
                        return ServiceResult<PerguntaAtualResponse>.Fail(
                            "NOT_FOUND",
                            "Pergunta não encontrada"
                        );
                    proximaOrdem = perguntaEspecifica.Ordem;
                }
                else
                {
                    // Avançar para próxima pergunta
                    if (sessao.OrdemPerguntaAtual.HasValue)
                    {
                        proximaOrdem = sessao.OrdemPerguntaAtual.Value + 1;
                        if (proximaOrdem > totalPerguntas)
                            return ServiceResult<PerguntaAtualResponse>.Fail(
                                "END_OF_SURVEY",
                                "Já estamos na última pergunta"
                            );
                    }
                    else
                    {
                        proximaOrdem = 1; // Primeira pergunta
                    }
                }

                var proximaPergunta = perguntas.FirstOrDefault(p => p.Ordem == proximaOrdem);
                if (proximaPergunta == null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Próxima pergunta não encontrada"
                    );

                // Atualizar sessão
                sessao.PerguntaAtualId = proximaPergunta.PerguntaId;
                sessao.OrdemPerguntaAtual = proximaPergunta.Ordem;
                sessao.PerguntaAtiva = request.AtivarPergunta;

                await _ctx.SaveChangesAsync(ct);

                var resposta = new PerguntaAtualResponse
                {
                    PerguntaId = proximaPergunta.PerguntaId,
                    Ordem = proximaPergunta.Ordem,
                    Texto = proximaPergunta.Texto,
                    Tipo = proximaPergunta.TipoPerguntaId.ToString(),
                    Ativa = sessao.PerguntaAtiva,
                    TotalPerguntas = totalPerguntas,
                    TemProxima = proximaPergunta.Ordem < totalPerguntas,
                    TemAnterior = proximaPergunta.Ordem > 1,
                    Opcoes = proximaPergunta.OpcoesPergunta
                        .OrderBy(o => o.Ordem)
                        .Select(o => new OpcaoInterativaResponse
                        {
                            OpcaoId = o.OpcaoId,
                            Texto = o.Texto,
                            Ordem = o.Ordem,
                        })
                        .ToList(),
                };

                return ServiceResult<PerguntaAtualResponse>.Ok(resposta);
            }
            catch (Exception ex)
            {
                return ServiceResult<PerguntaAtualResponse>.Fail(
                    "ERROR",
                    $"Erro ao avançar pergunta: {ex.Message}"
                );
            }
        }

        // =========================
        // VOLTAR PERGUNTA
        // =========================
        public async Task<ServiceResult<PerguntaAtualResponse>> VoltarPerguntaAsync(
            VoltarPerguntaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                if (!sessao.OrdemPerguntaAtual.HasValue || sessao.OrdemPerguntaAtual.Value <= 1)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "BEGINNING_OF_SURVEY",
                        "Já estamos na primeira pergunta"
                    );

                var perguntas = sessao.Pesquisa?.Perguntas.OrderBy(p => p.Ordem).ToList() ?? new List<Perguntas>();
                var perguntaAnterior = perguntas.FirstOrDefault(p => p.Ordem == sessao.OrdemPerguntaAtual.Value - 1);

                if (perguntaAnterior == null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Pergunta anterior não encontrada"
                    );

                // Atualizar sessão
                sessao.PerguntaAtualId = perguntaAnterior.PerguntaId;
                sessao.OrdemPerguntaAtual = perguntaAnterior.Ordem;
                sessao.PerguntaAtiva = request.AtivarPergunta;

                await _ctx.SaveChangesAsync(ct);

                var resposta = new PerguntaAtualResponse
                {
                    PerguntaId = perguntaAnterior.PerguntaId,
                    Ordem = perguntaAnterior.Ordem,
                    Texto = perguntaAnterior.Texto,
                    Tipo = perguntaAnterior.TipoPerguntaId.ToString(),
                    Ativa = sessao.PerguntaAtiva,
                    TotalPerguntas = perguntas.Count,
                    TemProxima = perguntaAnterior.Ordem < perguntas.Count,
                    TemAnterior = perguntaAnterior.Ordem > 1,
                    Opcoes = perguntaAnterior.OpcoesPergunta
                        .OrderBy(o => o.Ordem)
                        .Select(o => new OpcaoInterativaResponse
                        {
                            OpcaoId = o.OpcaoId,
                            Texto = o.Texto,
                            Ordem = o.Ordem,
                        })
                        .ToList(),
                };

                return ServiceResult<PerguntaAtualResponse>.Ok(resposta);
            }
            catch (Exception ex)
            {
                return ServiceResult<PerguntaAtualResponse>.Fail(
                    "ERROR",
                    $"Erro ao voltar pergunta: {ex.Message}"
                );
            }
        }

        // =========================
        // IR PARA PERGUNTA ESPECÍFICA
        // =========================
        public async Task<ServiceResult<PerguntaAtualResponse>> IrParaPerguntaAsync(
            IrParaPerguntaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas.OrderBy(pg => pg.Ordem))
                    .ThenInclude(pg => pg.OpcoesPergunta.Where(o => o.Ativa))
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                var perguntas = sessao.Pesquisa?.Perguntas.OrderBy(p => p.Ordem).ToList() ?? new List<Perguntas>();
                var pergunta = perguntas.FirstOrDefault(p => p.PerguntaId == request.PerguntaId);

                if (pergunta == null)
                    return ServiceResult<PerguntaAtualResponse>.Fail(
                        "NOT_FOUND",
                        "Pergunta não encontrada"
                    );

                // Atualizar sessão
                sessao.PerguntaAtualId = pergunta.PerguntaId;
                sessao.OrdemPerguntaAtual = pergunta.Ordem;
                sessao.PerguntaAtiva = request.AtivarPergunta;

                await _ctx.SaveChangesAsync(ct);

                var resposta = new PerguntaAtualResponse
                {
                    PerguntaId = pergunta.PerguntaId,
                    Ordem = pergunta.Ordem,
                    Texto = pergunta.Texto,
                    Tipo = pergunta.TipoPerguntaId.ToString(),
                    Ativa = sessao.PerguntaAtiva,
                    TotalPerguntas = perguntas.Count,
                    TemProxima = pergunta.Ordem < perguntas.Count,
                    TemAnterior = pergunta.Ordem > 1,
                    Opcoes = pergunta.OpcoesPergunta
                        .OrderBy(o => o.Ordem)
                        .Select(o => new OpcaoInterativaResponse
                        {
                            OpcaoId = o.OpcaoId,
                            Texto = o.Texto,
                            Ordem = o.Ordem,
                        })
                        .ToList(),
                };

                return ServiceResult<PerguntaAtualResponse>.Ok(resposta);
            }
            catch (Exception ex)
            {
                return ServiceResult<PerguntaAtualResponse>.Fail(
                    "ERROR",
                    $"Erro ao ir para pergunta: {ex.Message}"
                );
            }
        }

        // =========================
        // ATIVAR/DESATIVAR PERGUNTA
        // =========================
        public async Task<ServiceResult<bool>> AtivarPerguntaAsync(
            AtivarPerguntaRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<bool>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou inativa"
                    );

                if (!sessao.PerguntaAtualId.HasValue)
                    return ServiceResult<bool>.Fail(
                        "NO_CURRENT_QUESTION",
                        "Não há pergunta atual definida"
                    );

                sessao.PerguntaAtiva = request.Ativar;
                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<bool>.Ok(request.Ativar);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao ativar/desativar pergunta: {ex.Message}"
                );
            }
        }

        // =========================
        // MÉTODOS EXISTENTES (mantidos)
        // =========================
        public async Task<ServiceResult<object>> ObterResultadosTempoRealAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                    .ThenInclude(p => p.Perguntas)
                    .ThenInclude(pg => pg.OpcoesPergunta)
                    .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessaoId == sessaoId, ct);

                if (sessao is null)
                    return ServiceResult<object>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                var resultados = new
                {
                    SessaoId = sessao.SessaoId,
                    TotalParticipantes = sessao.ParticipantesSessao.Count,
                    PerguntaAtual = sessao.PerguntaAtualId,
                    PerguntaAtiva = sessao.PerguntaAtiva,
                    Participantes = sessao.ParticipantesSessao.Select(p => new
                    {
                        p.ParticipanteId,
                        p.NomeParticipante,
                        p.EntrouEm,
                        p.Ativo
                    }).ToList()
                };

                return ServiceResult<object>.Ok(resultados);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter resultados: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> FinalizarSessaoAsync(
            FinalizarSessaoRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao is null)
                    return ServiceResult<bool>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada ou já finalizada"
                    );

                sessao.Ativa = false;
                sessao.FinalizadaEm = DateTime.UtcNow;
                sessao.PerguntaAtiva = false;

                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao finalizar sessão: {ex.Message}"
                );
            }
        }

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
                    return ServiceResult<object>.Fail(
                        "NOT_FOUND",
                        "Sessão não encontrada"
                    );

                var status = new
                {
                    SessaoId = sessao.SessaoId,
                    Ativa = sessao.Ativa,
                    TotalParticipantes = sessao.ParticipantesSessao.Count,
                    PerguntaAtualId = sessao.PerguntaAtualId,
                    PerguntaAtiva = sessao.PerguntaAtiva,
                    ModoApresentacao = sessao.ModoApresentacao,
                    CriadaEm = sessao.CriadaEm,
                    IniciadaEm = sessao.IniciadaEm,
                    FinalizadaEm = sessao.FinalizadaEm
                };

                return ServiceResult<object>.Ok(status);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail(
                    "ERROR",
                    $"Erro ao obter status: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<List<ParticipanteResponse>>> ObterParticipantesAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            try
            {
                var participantes = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.ParticipantesSessao>()
                    .Where(p => p.SessaoId == sessaoId && p.SaiuEm == null)
                    .AsNoTracking()
                    .ToListAsync(ct);

                var resp = participantes.Select(p => new ParticipanteResponse
                {
                    ParticipanteId = p.ParticipanteId,
                    Nome = p.NomeParticipante,
                    SessaoId = p.SessaoId,
                    EntrouEm = p.EntrouEm,
                    Ativo = p.Ativo,
                }).ToList();

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
        // MÉTODOS AUXILIARES
        // =========================
        private async Task<string> GerarCodigoUnicoAsync(CancellationToken ct)
        {
            string codigo;
            bool codigoExiste;
            do
            {
                codigo = GerarCodigoAleatorio();
                codigoExiste = await _ctx.Set<SessoesInterativas>()
                    .AnyAsync(s => s.CodigoAcesso == codigo, ct);
            } while (codigoExiste);

            return codigo;
        }

        private static string GerarCodigoAleatorio()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
