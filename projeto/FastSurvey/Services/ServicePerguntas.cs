// FASTSURVEY/Services/ServicePerguntas.cs
using FASTSURVEY.Models;            // PesquisaVM
using FASTSURVEY.Models.DTO;        // PerguntaDiscursivaDto, PerguntaObjetivaDto, PerguntaMultiplaEscolhaDto, OpcaoDto
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
    public class ServicePerguntas
    {
        private readonly FastSurveyContext _context;

        private RepositoryPerguntas _repoPerguntas { get; }
        private RepositoryTipoPergunta _repoTipoPergunta { get; }
        private RepositoryOpcoesPergunta _repoOpcoes { get; }

        // IDs canônicos (ajuste conforme sua base)
        private const int TipoDiscursiva = 1;
        private const int TipoObjetiva = 2;
        private const int TipoMultipla = 3;

        public ServicePerguntas(FastSurveyContext context)
        {
            _context = context;
            _repoPerguntas = new RepositoryPerguntas(_context, true);
            _repoTipoPergunta = new RepositoryTipoPergunta(_context, true);
            _repoOpcoes = new RepositoryOpcoesPergunta(_context, true);
        }

        // =========================
        // CRUD básico
        // =========================

        public async Task<List<perguntas>> ListarTodasPerguntasAsync(CancellationToken ct = default)
        {
            return await _context.perguntas
                .AsNoTracking()
                .OrderBy(p => p.perguntaid)
                .ToListAsync(ct);
        }

        public async Task<List<perguntas>> ListarPorPesquisaAsync(int pesquisaId, CancellationToken ct = default)
        {
            return await _context.perguntas
                .AsNoTracking()
                .Where(p => p.pesquisaid == pesquisaId)
                .Include(p => p.opcoespergunta)
                .OrderBy(p => p.perguntaid)
                .ToListAsync(ct);
        }

        public async Task<perguntas?> BuscarPerguntaPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.perguntas
                .AsNoTracking()
                .Include(p => p.opcoespergunta)
                .FirstOrDefaultAsync(p => p.perguntaid == id, ct);
        }

        public async Task<perguntas> CadastrarPerguntaAsync(perguntas pergunta, CancellationToken ct = default)
        {
            NormalizaPergunta(pergunta);
            return await _repoPerguntas.IncluirAsync(pergunta); // repo não tem ct
        }

        public async Task<perguntas> AtualizarPerguntaAsync(perguntas pergunta, CancellationToken ct = default)
        {
            NormalizaPergunta(pergunta);
            return await _repoPerguntas.AlterarAsync(pergunta); // repo não tem ct
        }

        public async Task ExcluirPerguntaAsync(int id, CancellationToken ct = default)
        {
            var pergunta = await _repoPerguntas.SelecionarChaveAsync(id); // repo não tem ct
            if (pergunta == null) return;

            await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await DeleteOpcoesPerguntaAsync(id, ct);
                await _repoPerguntas.ExcluirAsync(pergunta); // repo não tem ct
                await _context.Database.CommitTransactionAsync(ct);
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync(ct);
                throw;
            }
        }

        // =========================
        // Criação por tipo (DTOs do front)
        // =========================

        public async Task<perguntas> CadastrarDiscursivaAsync(int pesquisaId, PerguntaDiscursivaDto dto, CancellationToken ct = default)
        {
            if (!ValidTexto(dto?.Texto)) throw new ArgumentException("Pergunta discursiva inválida (texto obrigatório).");

            var p = new perguntas
            {
                pesquisaid = pesquisaId,
                tipoperguntaid = TipoDiscursiva,
                texto = dto!.Texto!.Trim(),
                temgabarito = false,
                permitemultiplaselecao = false
            };

            return await _repoPerguntas.IncluirAsync(p); // repo não tem ct
        }

        public async Task<perguntas> CadastrarObjetivaAsync(int pesquisaId, PerguntaObjetivaDto dto, CancellationToken ct = default)
        {
            if (!ValidTexto(dto?.Texto)) throw new ArgumentException("Pergunta objetiva inválida (texto obrigatório).");

            var opcoesLimpas = LimpaOpcoes(dto?.Opcoes);
            if (opcoesLimpas.Count < 2)
                throw new ArgumentException("Pergunta objetiva requer pelo menos 2 opções.");

            int? idxCorreta = dto!.TemGabarito ? dto.CorretaIndex : null;
            if (idxCorreta.HasValue && (idxCorreta.Value < 0 || idxCorreta.Value >= opcoesLimpas.Count))
                throw new ArgumentOutOfRangeException(nameof(dto.CorretaIndex), "Índice de resposta correta inválido.");

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var p = new perguntas
                {
                    pesquisaid = pesquisaId,
                    tipoperguntaid = TipoObjetiva,
                    texto = dto.Texto!.Trim(),
                    temgabarito = idxCorreta.HasValue,
                    permitemultiplaselecao = false
                };
                await _context.perguntas.AddAsync(p, ct);
                await _context.SaveChangesAsync(ct); // precisa do ID

                var ops = opcoesLimpas.Select((o, i) => new opcoespergunta
                {
                    perguntaid = p.perguntaid,
                    texto = o,
                    correta = idxCorreta.HasValue && i == idxCorreta.Value
                }).ToList();

                await _context.opcoespergunta.AddRangeAsync(ops, ct);
                await _context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return p;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<perguntas> CadastrarMultiplaAsync(int pesquisaId, PerguntaMultiplaEscolhaDto dto, CancellationToken ct = default)
        {
            if (!ValidTexto(dto?.Texto)) throw new ArgumentException("Pergunta múltipla inválida (texto obrigatório).");

            var opcoesLimpas = LimpaOpcoes(dto?.Opcoes);
            if (opcoesLimpas.Count < 2)
                throw new ArgumentException("Pergunta múltipla requer pelo menos 2 opções.");

            var corretasIdx = new HashSet<int>(
                (dto?.Corretas ?? Enumerable.Empty<int>())
                .Distinct()
                .Where(i => i >= 0 && i < opcoesLimpas.Count));

            if (!dto!.PermitirMultiplaSelecao && corretasIdx.Count > 1)
                corretasIdx = new HashSet<int>(new[] { corretasIdx.Min() });

            var temGabarito = dto.TemGabarito && corretasIdx.Count > 0;

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var p = new perguntas
                {
                    pesquisaid = pesquisaId,
                    tipoperguntaid = TipoMultipla,
                    texto = dto.Texto!.Trim(),
                    temgabarito = temGabarito,
                    permitemultiplaselecao = dto.PermitirMultiplaSelecao
                };
                await _context.perguntas.AddAsync(p, ct);
                await _context.SaveChangesAsync(ct); // precisa do ID

                var ops = opcoesLimpas.Select((o, i) => new opcoespergunta
                {
                    perguntaid = p.perguntaid,
                    texto = o,
                    correta = temGabarito && corretasIdx.Contains(i)
                }).ToList();

                await _context.opcoespergunta.AddRangeAsync(ops, ct);
                await _context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return p;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        // =========================
        // Helpers de opções / gabarito
        // =========================

        /// <summary>Substitui TODAS as opções de uma pergunta por novas. Mantém consistência do gabarito conforme o tipo.</summary>
        public async Task RecriarOpcoesAsync(int perguntaId, IEnumerable<(string texto, bool correta)> novas, CancellationToken ct = default)
        {
            var pergunta = await _context.perguntas.FirstOrDefaultAsync(p => p.perguntaid == perguntaId, ct)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");

            var novasList = (novas ?? Enumerable.Empty<(string, bool)>())
                .Select(x => ((x.texto ?? string.Empty).Trim(), x.correta))
                .Where(x => !string.IsNullOrWhiteSpace(x.Item1))
                .ToList();

            if (novasList.Count < 2 && pergunta.tipoperguntaid != TipoDiscursiva)
                throw new ArgumentException("Perguntas de escolha exigem pelo menos 2 opções.");

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await DeleteOpcoesPerguntaAsync(perguntaId, ct);

                // Objetiva: no máximo uma correta
                if (pergunta.tipoperguntaid == TipoObjetiva)
                {
                    var firstIdx = novasList.FindIndex(x => x.correta);
                    for (int i = 0; i < novasList.Count; i++)
                        novasList[i] = (novasList[i].Item1, i == firstIdx && firstIdx >= 0);
                    pergunta.temgabarito = firstIdx >= 0;
                }
                else if (pergunta.tipoperguntaid == TipoMultipla)
                {
                    pergunta.temgabarito = novasList.Any(x => x.correta);
                }
                else
                {
                    pergunta.temgabarito = false; // discursiva
                }

                var novasOps = novasList.Select(x => new opcoespergunta
                {
                    perguntaid = perguntaId,
                    texto = x.Item1,
                    correta = pergunta.tipoperguntaid != TipoDiscursiva && x.correta
                }).ToList();

                if (novasOps.Count > 0)
                    await _context.opcoespergunta.AddRangeAsync(novasOps, ct);

                _context.perguntas.Update(pergunta);
                await _context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>Define a opção correta (objetiva). Passar null para limpar gabarito.</summary>
        public async Task AtualizarGabaritoObjetivaAsync(int perguntaId, int? corretaIndex, CancellationToken ct = default)
        {
            var pergunta = await _context.perguntas.FirstOrDefaultAsync(p => p.perguntaid == perguntaId, ct)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");
            if (pergunta.tipoperguntaid != TipoObjetiva)
                throw new InvalidOperationException("Pergunta não é do tipo objetiva.");

            var opcoes = await _context.opcoespergunta
                .Where(o => o.perguntaid == perguntaId)
                .OrderBy(o => o.opcaoid)
                .ToListAsync(ct);

            if (corretaIndex.HasValue && (corretaIndex.Value < 0 || corretaIndex.Value >= opcoes.Count))
                throw new ArgumentOutOfRangeException(nameof(corretaIndex), "Índice de resposta correta inválido.");

            for (int i = 0; i < opcoes.Count; i++)
                opcoes[i].correta = (corretaIndex.HasValue && i == corretaIndex.Value);

            pergunta.temgabarito = corretaIndex.HasValue;

            _context.perguntas.Update(pergunta);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>Define múltiplas corretas (múltipla escolha). Lista vazia/null: limpa gabarito.</summary>
        public async Task AtualizarGabaritoMultiplaAsync(int perguntaId, IEnumerable<int> corretasIdx, CancellationToken ct = default)
        {
            var pergunta = await _context.perguntas.FirstOrDefaultAsync(p => p.perguntaid == perguntaId, ct)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");
            if (pergunta.tipoperguntaid != TipoMultipla)
                throw new InvalidOperationException("Pergunta não é do tipo múltipla.");

            var opcoes = await _context.opcoespergunta
                .Where(o => o.perguntaid == perguntaId)
                .OrderBy(o => o.opcaoid)
                .ToListAsync(ct);

            var set = new HashSet<int>((corretasIdx ?? Enumerable.Empty<int>()).Distinct()
                .Where(i => i >= 0 && i < opcoes.Count));

            if (!pergunta.permitemultiplaselecao && set.Count > 1)
                set = new HashSet<int>(new[] { set.Min() });

            for (int i = 0; i < opcoes.Count; i++)
                opcoes[i].correta = set.Contains(i);

            pergunta.temgabarito = set.Count > 0;

            _context.perguntas.Update(pergunta);
            await _context.SaveChangesAsync(ct);
        }

        // =========================
        // Montagem em massa a partir da PesquisaVM
        // =========================
        public async Task<List<perguntas>> CriarPerguntasAPartirDaVMAsync(int pesquisaId, PesquisaVM vm, CancellationToken ct = default)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            var criadas = new List<perguntas>();

            foreach (var d in vm.PerguntasDiscursivas ?? Enumerable.Empty<PerguntaDiscursivaDto>())
                criadas.Add(await CadastrarDiscursivaAsync(pesquisaId, d, ct));

            foreach (var o in vm.PerguntasObjetivas ?? Enumerable.Empty<PerguntaObjetivaDto>())
                criadas.Add(await CadastrarObjetivaAsync(pesquisaId, o, ct));

            foreach (var m in vm.PerguntasMultiplaEscolha ?? Enumerable.Empty<PerguntaMultiplaEscolhaDto>())
                criadas.Add(await CadastrarMultiplaAsync(pesquisaId, m, ct));

            return criadas;
        }

        // =========================
        // Privados utilitários
        // =========================

        private static bool ValidTexto(string? t) => !string.IsNullOrWhiteSpace(t);

        private static void NormalizaPergunta(perguntas p)
        {
            if (p == null) return;
            p.texto = (p.texto ?? string.Empty).Trim();

            // Normaliza flags por tipo
            if (p.tipoperguntaid == TipoDiscursiva)
            {
                p.temgabarito = false;
                p.permitemultiplaselecao = false;
            }
            else if (p.tipoperguntaid == TipoObjetiva)
            {
                p.permitemultiplaselecao = false; // objetiva sempre single-select
            }
            // TipoMultipla mantém valores enviados
        }

        private static List<string> LimpaOpcoes(IEnumerable<OpcaoDto>? opcoes)
        {
            return (opcoes ?? Enumerable.Empty<OpcaoDto>())
                .Select(o => (o?.Texto ?? string.Empty).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }

        private async Task DeleteOpcoesPerguntaAsync(int perguntaId, CancellationToken ct)
        {
            var query = _context.opcoespergunta.Where(o => o.perguntaid == perguntaId);
#if NET7_0_OR_GREATER
            await query.ExecuteDeleteAsync(ct);
#else
            var antigas = await query.ToListAsync(ct);
            if (antigas.Count > 0)
            {
                _context.opcoespergunta.RemoveRange(antigas);
                await _context.SaveChangesAsync(ct);
            }
#endif
        }
    }
}
