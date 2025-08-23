using FASTSURVEY.Controllers;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Text.Json;

namespace FASTSURVEY.Services.PesquisaInterativa
{
    public class PesquisaInterativaService : IPesquisaInterativaService
    {
        private readonly FastSurveyContext _ctx;
        private static readonly Random _random = new();

        public PesquisaInterativaService(FastSurveyContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ServiceResult<SessaoInterativaResponse>> IniciarSessaoAsync(PesquisaInterativaRequest request, CancellationToken ct = default)
        {
            try
            {
                var pesquisa = await _ctx.Pesquisas
                    .Include(p => p.Perguntas)
                        .ThenInclude(pg => pg.Opcoespergunta.Where(o => o.Ativa))
                    .FirstOrDefaultAsync(p => p.Pesquisaid == request.PesquisaId, ct);

                if (pesquisa == null)
                    return ServiceResult<SessaoInterativaResponse>.Fail("NOT_FOUND", "Pesquisa não encontrada");

                if (!pesquisa.Isinterativa)
                    return ServiceResult<SessaoInterativaResponse>.Fail("NOT_INTERACTIVE", "Esta pesquisa não é interativa");

                // Verificar se já existe uma sessão ativa para esta pesquisa
                var sessaoExistente = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.PesquisaId == request.PesquisaId && s.Ativa, ct);

                if (sessaoExistente != null)
                {
                    return ServiceResult<SessaoInterativaResponse>.Fail("ACTIVE_SESSION", 
                        $"Já existe uma sessão ativa para esta pesquisa. Código: {sessaoExistente.CodigoAcesso}");
                }

                // Gerar código único de acesso
                string codigoAcesso;
                do
                {
                    codigoAcesso = GerarCodigoAcesso();
                } while (await _ctx.Set<SessoesInterativas>().AnyAsync(s => s.CodigoAcesso == codigoAcesso && s.Ativa, ct));

                var sessaoId = Guid.NewGuid().ToString();
                var agora = DateTime.UtcNow;

                var novaSessao = new SessoesInterativas
                {
                    SessaoId = sessaoId,
                    PesquisaId = request.PesquisaId,
                    CodigoAcesso = codigoAcesso,
                    CriadaEm = agora,
                    IniciadaEm = request.IsAtiva ? agora : null,
                    Ativa = true
                };

                _ctx.Set<SessoesInterativas>().Add(novaSessao);
                await _ctx.SaveChangesAsync(ct);

                var response = new SessaoInterativaResponse
                {
                    SessaoId = sessaoId,
                    CodigoAcesso = codigoAcesso,
                    PesquisaId = pesquisa.Pesquisaid,
                    TituloPesquisa = pesquisa.Titulo,
                    Ativa = true,
                    CriadaEm = agora,
                    IniciadaEm = novaSessao.IniciadaEm,
                    TotalParticipantes = 0,
                    Perguntas = pesquisa.Perguntas
                        .OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.Perguntaid,
                            Texto = p.Texto,
                            Tipo = p.Tipoperguntaid,
                            Ordem = p.Ordem,
                            Opcoes = p.Opcoespergunta
                                .OrderBy(o => o.Ordem)
                                .Select(o => new OpcaoInterativaResponse
                                {
                                    OpcaoId = o.Opcaoid,
                                    Texto = o.Texto,
                                    Ordem = o.Ordem
                                }).ToList()
                        }).ToList()
                };

                return ServiceResult<SessaoInterativaResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<SessaoInterativaResponse>.Fail("ERROR", $"Erro ao iniciar sessão: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ParticipanteResponse>> EntrarSessaoAsync(EntrarSessaoRequest request, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.CodigoAcesso == request.CodigoAcesso && s.Ativa, ct);

                if (sessao == null)
                    return ServiceResult<ParticipanteResponse>.Fail("NOT_FOUND", "Sessão não encontrada ou inativa");

                // Verificar se o participante já está na sessão
                var participanteExistente = await _ctx.Set<ParticipantesSessao>()
                    .FirstOrDefaultAsync(p => p.SessaoId == sessao.SessaoId && 
                        p.NomeParticipante == request.NomeParticipante && p.SaiuEm == null, ct);

                if (participanteExistente != null)
                {
                    return ServiceResult<ParticipanteResponse>.Ok(new ParticipanteResponse
                    {
                        ParticipanteId = participanteExistente.ParticipanteId,
                        Nome = participanteExistente.NomeParticipante,
                        SessaoId = sessao.SessaoId,
                        EntrouEm = participanteExistente.EntrouEm,
                        Ativo = true
                    });
                }

                var novoParticipante = new ParticipantesSessao
                {
                    SessaoId = sessao.SessaoId,
                    NomeParticipante = request.NomeParticipante,
                    EntrouEm = DateTime.UtcNow
                };

                _ctx.Set<ParticipantesSessao>().Add(novoParticipante);
                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<ParticipanteResponse>.Ok(new ParticipanteResponse
                {
                    ParticipanteId = novoParticipante.ParticipanteId,
                    Nome = novoParticipante.NomeParticipante,
                    SessaoId = sessao.SessaoId,
                    EntrouEm = novoParticipante.EntrouEm,
                    Ativo = true
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<ParticipanteResponse>.Fail("ERROR", $"Erro ao entrar na sessão: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SessaoInterativaResponse>> ObterSessaoAsync(string codigo, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                        .ThenInclude(p => p.Perguntas)
                            .ThenInclude(pg => pg.Opcoespergunta.Where(o => o.Ativa))
                    .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                    .FirstOrDefaultAsync(s => s.CodigoAcesso == codigo, ct);

                if (sessao == null)
                    return ServiceResult<SessaoInterativaResponse>.Fail("NOT_FOUND", "Sessão não encontrada");

                var response = new SessaoInterativaResponse
                {
                    SessaoId = sessao.SessaoId,
                    CodigoAcesso = sessao.CodigoAcesso,
                    PesquisaId = sessao.PesquisaId,
                    TituloPesquisa = sessao.Pesquisa.Titulo,
                    Ativa = sessao.Ativa,
                    CriadaEm = sessao.CriadaEm,
                    IniciadaEm = sessao.IniciadaEm,
                    FinalizadaEm = sessao.FinalizadaEm,
                    TotalParticipantes = sessao.ParticipantesSessao.Count,
                    Perguntas = sessao.Pesquisa.Perguntas
                        .OrderBy(p => p.Ordem)
                        .Select(p => new PerguntaInterativaResponse
                        {
                            PerguntaId = p.Perguntaid,
                            Texto = p.Texto,
                            Tipo = p.Tipoperguntaid,
                            Ordem = p.Ordem,
                            Opcoes = p.Opcoespergunta
                                .OrderBy(o => o.Ordem)
                                .Select(o => new OpcaoInterativaResponse
                                {
                                    OpcaoId = o.Opcaoid,
                                    Texto = o.Texto,
                                    Ordem = o.Ordem
                                }).ToList()
                        }).ToList()
                };

                return ServiceResult<SessaoInterativaResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<SessaoInterativaResponse>.Fail("ERROR", $"Erro ao obter sessão: {ex.Message}");
            }
        }

        public async Task<ServiceResult<RespostaInterativaResponse>> ResponderPerguntaAsync(RespostaInterativaRequest request, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId && s.Ativa, ct);

                if (sessao == null)
                    return ServiceResult<RespostaInterativaResponse>.Fail("NOT_FOUND", "Sessão não encontrada ou inativa");

                var pergunta = await _ctx.Perguntas
                    .Include(p => p.Opcoespergunta)
                    .FirstOrDefaultAsync(p => p.Perguntaid == request.PerguntaId, ct);

                if (pergunta == null)
                    return ServiceResult<RespostaInterativaResponse>.Fail("NOT_FOUND", "Pergunta não encontrada");

                // Criar resposta
                var novaResposta = new Respostas
                {
                    Perguntaid = request.PerguntaId,
                    Texto = request.TextoResposta ?? string.Empty,
                    Dataresposta = DateTime.UtcNow,
                    SessaoId = request.SessaoId,
                    RespostaAnonima = true // Respostas interativas são sempre anônimas
                };

                _ctx.Respostas.Add(novaResposta);
                await _ctx.SaveChangesAsync(ct);

                // Associar opções selecionadas
                if (request.OpcoesSelecionadas.Any())
                {
                    var opcoes = await _ctx.Set<Opcoespergunta>()
                        .Where(o => request.OpcoesSelecionadas.Contains(o.Opcaoid))
                        .ToListAsync(ct);

                    foreach (var opcao in opcoes)
                    {
                        novaResposta.Opcao.Add(opcao);
                    }
                    await _ctx.SaveChangesAsync(ct);
                }

                // Obter resultados em tempo real
                var resultados = await ObterResultadosTempoRealAsync(request.SessaoId, ct);

                return ServiceResult<RespostaInterativaResponse>.Ok(new RespostaInterativaResponse
                {
                    Sucesso = true,
                    Mensagem = "Resposta registrada com sucesso",
                    RespostaId = novaResposta.Respostaid,
                    ResultadosTempoReal = resultados.Success ? resultados.Data : null
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<RespostaInterativaResponse>.Fail("ERROR", $"Erro ao responder pergunta: {ex.Message}");
            }
        }

        public async Task<ServiceResult<object>> ObterResultadosTempoRealAsync(string sessaoId, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.Pesquisa)
                        .ThenInclude(p => p.Perguntas)
                            .ThenInclude(pg => pg.Opcoespergunta)
                    .FirstOrDefaultAsync(s => s.SessaoId == sessaoId, ct);

                if (sessao == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Sessão não encontrada");

                var resultados = sessao.Pesquisa.Perguntas
                    .OrderBy(p => p.Ordem)
                    .Select(pergunta => new
                    {
                        perguntaId = pergunta.Perguntaid,
                        texto = pergunta.Texto,
                        totalRespostas = _ctx.Respostas.Count(r => r.Perguntaid == pergunta.Perguntaid && r.SessaoId == sessaoId),
                        opcoes = pergunta.Opcoespergunta.OrderBy(o => o.Ordem).Select(opcao => new
                        {
                            opcaoId = opcao.Opcaoid,
                            texto = opcao.Texto,
                            totalRespostas = _ctx.Set<Respostas>()
                                .Where(r => r.Perguntaid == pergunta.Perguntaid && r.SessaoId == sessaoId)
                                .SelectMany(r => r.Opcao)
                                .Count(o => o.Opcaoid == opcao.Opcaoid)
                        }).ToList()
                    }).ToList();

                var totalParticipantes = await _ctx.Set<ParticipantesSessao>()
                    .CountAsync(p => p.SessaoId == sessaoId && p.SaiuEm == null, ct);

                return ServiceResult<object>.Ok(new
                {
                    sessaoId,
                    totalParticipantes,
                    perguntas = resultados
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter resultados: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> FinalizarSessaoAsync(FinalizarSessaoRequest request, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .FirstOrDefaultAsync(s => s.SessaoId == request.SessaoId, ct);

                if (sessao == null)
                    return ServiceResult<bool>.Fail("NOT_FOUND", "Sessão não encontrada");

                sessao.FinalizadaEm = DateTime.UtcNow;
                sessao.Ativa = false;

                // Marcar saída de todos os participantes
                var participantes = await _ctx.Set<ParticipantesSessao>()
                    .Where(p => p.SessaoId == request.SessaoId && p.SaiuEm == null)
                    .ToListAsync(ct);

                foreach (var participante in participantes)
                {
                    participante.SaiuEm = DateTime.UtcNow;
                }

                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("ERROR", $"Erro ao finalizar sessão: {ex.Message}");
            }
        }

        public async Task<ServiceResult<object>> ObterStatusSessaoAsync(string sessaoId, CancellationToken ct = default)
        {
            try
            {
                var sessao = await _ctx.Set<SessoesInterativas>()
                    .Include(s => s.ParticipantesSessao.Where(p => p.SaiuEm == null))
                    .FirstOrDefaultAsync(s => s.SessaoId == sessaoId, ct);

                if (sessao == null)
                    return ServiceResult<object>.Fail("NOT_FOUND", "Sessão não encontrada");

                var status = new
                {
                    sessaoId = sessao.SessaoId,
                    codigoAcesso = sessao.CodigoAcesso,
                    ativa = sessao.Ativa,
                    criadaEm = sessao.CriadaEm,
                    iniciadaEm = sessao.IniciadaEm,
                    finalizadaEm = sessao.FinalizadaEm,
                    totalParticipantes = sessao.ParticipantesSessao.Count,
                    participantesAtivos = sessao.ParticipantesSessao.Count(p => p.SaiuEm == null)
                };

                return ServiceResult<object>.Ok(status);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail("ERROR", $"Erro ao obter status: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ParticipanteResponse>>> ObterParticipantesAsync(string sessaoId, CancellationToken ct = default)
        {
            try
            {
                var participantes = await _ctx.Set<ParticipantesSessao>()
                    .Where(p => p.SessaoId == sessaoId)
                    .Select(p => new ParticipanteResponse
                    {
                        ParticipanteId = p.ParticipanteId,
                        Nome = p.NomeParticipante,
                        SessaoId = p.SessaoId,
                        EntrouEm = p.EntrouEm,
                        Ativo = p.SaiuEm == null
                    })
                    .ToListAsync(ct);

                return ServiceResult<List<ParticipanteResponse>>.Ok(participantes);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ParticipanteResponse>>.Fail("ERROR", $"Erro ao obter participantes: {ex.Message}");
            }
        }

        private string GerarCodigoAcesso()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }

    // Classes temporárias para as entidades que serão geradas pelo EF
    public class SessoesInterativas
    {
        public string SessaoId { get; set; } = string.Empty;
        public int PesquisaId { get; set; }
        public string CodigoAcesso { get; set; } = string.Empty;
        public DateTime CriadaEm { get; set; }
        public DateTime? IniciadaEm { get; set; }
        public DateTime? FinalizadaEm { get; set; }
        public bool Ativa { get; set; }
        public Pesquisas Pesquisa { get; set; } = null!;
        public List<ParticipantesSessao> ParticipantesSessao { get; set; } = new();
    }

    public class ParticipantesSessao
    {
        public int ParticipanteId { get; set; }
        public string SessaoId { get; set; } = string.Empty;
        public string NomeParticipante { get; set; } = string.Empty;
        public DateTime EntrouEm { get; set; }
        public DateTime? SaiuEm { get; set; }
    }
}
