// FASTSURVEY/Services/ServiceRespostas.cs
using FASTSURVEY.Models.DTO;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Services
{
    public class ServiceRespostas
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryRespostas _repoRespostas;

        // IDs canônicos (ajuste se necessário)
        private const int TipoDiscursiva = 1;
        private const int TipoObjetiva = 2;
        private const int TipoMultipla = 3;

        public ServiceRespostas(FastSurveyContext context, RepositoryRespostas repositoryRespostas)
        {
            _context = context;
            _repoRespostas = repositoryRespostas;
        }

        // ===== CRUD básico =====
        public async Task<List<respostas>> ListarTodasRespostasAsync(CancellationToken ct = default)
            => await _context.respostas.AsNoTracking().OrderBy(r => r.respostaid).ToListAsync(ct);

        public async Task<respostas?> ObterRespostaPorIdAsync(int id, CancellationToken ct = default)
            => await _context.respostas.AsNoTracking().FirstOrDefaultAsync(r => r.respostaid == id, ct);

        public async Task<respostas> CadastrarRespostaAsync(respostas resposta, CancellationToken ct = default)
        {
            if (resposta == null) throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");
            _context.respostas.Add(resposta);
            await _context.SaveChangesAsync(ct);
            return resposta;
        }

        public async Task<respostas> AtualizarRespostaAsync(respostas resposta, CancellationToken ct = default)
        {
            if (resposta == null) throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");
            var existente = await _context.respostas.FindAsync(new object[] { resposta.respostaid }, ct);
            if (existente == null) throw new KeyNotFoundException($"Resposta {resposta.respostaid} não encontrada.");
            _context.Entry(existente).CurrentValues.SetValues(resposta);
            await _context.SaveChangesAsync(ct);
            return resposta;
        }

        public async Task<bool> ExcluirRespostaAsync(int id, CancellationToken ct = default)
        {
            var resposta = await _context.respostas.FindAsync(new object[] { id }, ct);
            if (resposta == null) throw new KeyNotFoundException($"Resposta {id} não encontrada.");

            // Limpa vínculos primeiro (se não houver cascade)
            var vincs = await _context.respostas_opcoes.Where(v => v.respostaid == id).ToListAsync(ct);
            if (vincs.Count > 0) _context.respostas_opcoes.RemoveRange(vincs);

            _context.respostas.Remove(resposta);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // ===== Gravação em LOTE (RespostasLoteVM) =====

        /// <summary>
        /// Grava todas as respostas de uma pesquisa (discursivas/objetivas/múltiplas) de forma transacional.
        /// Valida: pergunta pertence à pesquisa, opções pertencem à pergunta e cardinalidade por tipo.
        /// </summary>
        public async Task<GravarLoteResultado> GravarLoteAsync(RespostasLoteVM vm, CancellationToken ct = default)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.PesquisaId <= 0) throw new ArgumentException("PesquisaId inválido.");
            if (vm.Itens == null || vm.Itens.Count == 0)
                return new GravarLoteResultado { Inseridas = 0, VinculosCriados = 0 };

            // Carrega perguntas e metadados necessários da pesquisa
            var perguntasMeta = await _context.perguntas
                .AsNoTracking()
                .Where(p => p.pesquisaid == vm.PesquisaId)
                .Select(p => new
                {
                    p.perguntaid,
                    p.tipoperguntaid,
                    p.permitemultiplaselecao
                })
                .ToDictionaryAsync(p => p.perguntaid, p => (tipo: p.tipoperguntaid, multipla: p.permitemultiplaselecao), ct);

            if (perguntasMeta.Count == 0)
                throw new InvalidOperationException($"Pesquisa {vm.PesquisaId} não possui perguntas.");

            // Pré-carrega opções por pergunta para validar pertencimento
            var opcoesPorPergunta = await _context.opcoespergunta
                .AsNoTracking()
                .Where(o => perguntasMeta.Keys.Contains(o.perguntaid))
                .GroupBy(o => o.perguntaid)
                .Select(g => new
                {
                    PerguntaId = g.Key,
                    Opcoes = g.Select(o => o.opcaoid).ToHashSet()
                })
                .ToDictionaryAsync(x => x.PerguntaId, x => x.Opcoes, ct);

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var novasRespostas = new List<respostas>();
                var pendentesVinculo = new List<(respostas Resp, List<int> OpcaoIds)>(); // para objetiva/múltipla

                foreach (var item in vm.Itens)
                {
                    var perguntaId = item.PerguntaId;

                    if (!perguntasMeta.TryGetValue(perguntaId, out var meta))
                        throw new KeyNotFoundException($"Pergunta {perguntaId} não pertence à pesquisa {vm.PesquisaId}.");

                    // (Opcional) validar coerência do item.Tipo com o tipo no banco
                    var tipoEsperado = meta.tipo switch { 1 => "discursiva", 2 => "objetiva", 3 => "multipla", _ => "desconhecido" };
                    var tipoEnviado = (item.Tipo ?? "").Trim().ToLowerInvariant();
                    if (tipoEsperado != "desconhecido" && tipoEnviado != tipoEsperado)
                        throw new ArgumentException($"Tipo enviado '{item.Tipo}' não corresponde ao tipo da pergunta {perguntaId} ({tipoEsperado}).");

                    switch (meta.tipo)
                    {
                        case TipoDiscursiva:
                            {
                                var resp = new respostas
                                {
                                    perguntaid = perguntaId,
                                    texto = item.Texto ?? string.Empty,
                                    // Se existir coluna loginid em 'respostas', descomente:
                                    // loginid = vm.LoginId
                                };
                                novasRespostas.Add(resp);
                                break;
                            }

                        case TipoObjetiva:
                            {
                                var selecionadas = (item.Opcoes ?? new List<int>()).Distinct().ToList();
                                if (selecionadas.Count != 1)
                                    throw new ArgumentException($"Pergunta objetiva {perguntaId} requer exatamente 1 opção.");

                                var opc = selecionadas[0];
                                if (!opcoesPorPergunta.TryGetValue(perguntaId, out var set) || !set.Contains(opc))
                                    throw new ArgumentException($"Opção {opc} não pertence à pergunta {perguntaId}.");

                                var resp = new respostas
                                {
                                    perguntaid = perguntaId,
                                    texto = string.Empty,
                                    // loginid = vm.LoginId
                                };
                                novasRespostas.Add(resp);
                                pendentesVinculo.Add((resp, new List<int> { opc }));
                                break;
                            }

                        case TipoMultipla:
                            {
                                var selecionadas = (item.Opcoes ?? new List<int>()).Distinct().ToList();

                                // Se a pergunta NÃO permite múltiplas, reduz para no máximo 1
                                if (!meta.multipla && selecionadas.Count > 1)
                                    selecionadas = new List<int> { selecionadas[0] };

                                if (selecionadas.Count > 0)
                                {
                                    if (!opcoesPorPergunta.TryGetValue(perguntaId, out var set))
                                        throw new ArgumentException($"A pergunta {perguntaId} não possui opções cadastradas.");
                                    foreach (var opc in selecionadas)
                                        if (!set.Contains(opc))
                                            throw new ArgumentException($"Opção {opc} não pertence à pergunta {perguntaId}.");
                                }

                                var resp = new respostas
                                {
                                    perguntaid = perguntaId,
                                    texto = string.Empty,
                                    // loginid = vm.LoginId
                                };
                                novasRespostas.Add(resp);
                                if (selecionadas.Count > 0)
                                    pendentesVinculo.Add((resp, selecionadas));
                                break;
                            }

                        default:
                            throw new InvalidOperationException($"Tipo de pergunta desconhecido: {meta.tipo}");
                    }
                }

                // ===== fase 1: insere respostas =====
                if (novasRespostas.Count > 0)
                {
                    await _context.respostas.AddRangeAsync(novasRespostas, ct);
                    await _context.SaveChangesAsync(ct); // gera respostaid
                }

                // ===== fase 2: vínculos respostas_opcoes =====
                var vinculos = new List<respostas_opcoes>();
                foreach (var (resp, opcoes) in pendentesVinculo)
                    foreach (var opc in opcoes)
                        vinculos.Add(new respostas_opcoes { respostaid = resp.respostaid, opcaoid = opc });

                if (vinculos.Count > 0)
                {
                    await _context.respostas_opcoes.AddRangeAsync(vinculos, ct);
                    await _context.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);

                return new GravarLoteResultado
                {
                    Inseridas = novasRespostas.Count,
                    VinculosCriados = vinculos.Count
                };
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }

    // DTO de retorno opcional para telemetria/UX
    public class GravarLoteResultado
    {
        public int Inseridas { get; set; }
        public int VinculosCriados { get; set; }
    }
}
