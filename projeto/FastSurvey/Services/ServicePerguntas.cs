using FASTSURVEY.Models;
using FASTSURVEY.Models.DTO; // PesquisaVM + DTOs de perguntas/opções
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Services
{
    public class ServicePerguntas
    {
        private readonly FastSurveyContext _context;

        private RepositoryPerguntas _repoPerguntas { get; }
        private RepositoryTipoPergunta _repoTipoPergunta { get; }
        private RepositoryOpcoesPergunta _repoOpcoes { get; }

        // IDs canônicos (ajuste se sua base estiver diferente)
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

        // ===== CRUD básico =====

        public async Task<List<perguntas>> ListarTodasPerguntasAsync()
        {
            return await _repoPerguntas.SelecionarTodosAsync();
        }

        public async Task<List<perguntas>> ListarPorPesquisaAsync(int pesquisaId)
        {
            // inclui opções para facilitar telas de edição
            return await _context.perguntas
                .AsNoTracking()
                .Where(p => p.pesquisaid == pesquisaId)
                .Include(p => p.opcoespergunta)
                .OrderBy(p => p.perguntaid)
                .ToListAsync();
        }

        public async Task<perguntas> BuscarPerguntaPorIdAsync(int id)
        {
            return await _repoPerguntas.SelecionarChaveAsync(id);
        }

        public async Task<perguntas> CadastrarPerguntaAsync(perguntas pergunta)
        {
            return await _repoPerguntas.IncluirAsync(pergunta);
        }

        public async Task<perguntas> AtualizarPerguntaAsync(perguntas pergunta)
        {
            return await _repoPerguntas.AlterarAsync(pergunta);
        }

        public async Task ExcluirPerguntaAsync(int id)
        {
            var pergunta = await _repoPerguntas.SelecionarChaveAsync(id);
            if (pergunta != null)
            {
                await _repoPerguntas.ExcluirAsync(pergunta);
            }
        }

        // ===== Criação por tipo (a partir dos DTOs do front) =====

        public async Task<perguntas> CadastrarDiscursivaAsync(int pesquisaId, PerguntaDiscursivaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Titulo))
                throw new ArgumentException("Pergunta discursiva inválida.");

            var p = new perguntas
            {
                pesquisaid = pesquisaId,
                tipoperguntaid = TipoDiscursiva,
                texto = dto.Titulo.Trim(),
                temgabarito = false,
                permitemultiplaselecao = false
            };

            return await _repoPerguntas.IncluirAsync(p);
        }

        public async Task<perguntas> CadastrarObjetivaAsync(int pesquisaId, PerguntaObjetivaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Titulo))
                throw new ArgumentException("Pergunta objetiva inválida.");

            var p = new perguntas
            {
                pesquisaid = pesquisaId,
                tipoperguntaid = TipoObjetiva,
                texto = dto.Titulo.Trim(),
                temgabarito = dto.TemGabarito,
                permitemultiplaselecao = false
            };
            await _repoPerguntas.IncluirAsync(p);

            // cria opções e marca correta se houver gabarito
            var opcoes = (dto.Opcoes ?? new List<OpcaoDto>())
                .Select((o, idx) => new opcoespergunta
                {
                    perguntaid = p.perguntaid,
                    texto = (o.Texto ?? string.Empty).Trim(),
                    correta = dto.TemGabarito && dto.CorretaIndex.HasValue && dto.CorretaIndex.Value == idx
                })
                .ToList();

            foreach (var op in opcoes)
                await _repoOpcoes.IncluirAsync(op);

            return p;
        }

        public async Task<perguntas> CadastrarMultiplaAsync(int pesquisaId, PerguntaMultiplaEscolhaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Titulo))
                throw new ArgumentException("Pergunta múltipla inválida.");

            var p = new perguntas
            {
                pesquisaid = pesquisaId,
                tipoperguntaid = TipoMultipla,
                texto = dto.Titulo.Trim(),
                temgabarito = dto.TemGabarito,
                permitemultiplaselecao = dto.PermitirMultiplaSelecao
            };
            await _repoPerguntas.IncluirAsync(p);

            var corretas = new HashSet<int>((dto.Corretas ?? new List<int>()).Distinct());

            var opcoes = (dto.Opcoes ?? new List<OpcaoDto>())
                .Select((o, idx) => new opcoespergunta
                {
                    perguntaid = p.perguntaid,
                    texto = (o.Texto ?? string.Empty).Trim(),
                    correta = dto.TemGabarito && corretas.Contains(idx)
                })
                .ToList();

            foreach (var op in opcoes)
                await _repoOpcoes.IncluirAsync(op);

            return p;
        }

        // ===== Helpers de gabarito/opções =====

        /// <summary>
        /// Substitui TODAS as opções de uma pergunta por novas (útil para edição).
        /// </summary>
        public async Task RecriarOpcoesAsync(int perguntaId, IEnumerable<(string texto, bool correta)> novas)
        {
            var pergunta = await _repoPerguntas.SelecionarChaveAsync(perguntaId)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");

            // apaga opções antigas
            var antigas = _context.opcoespergunta.Where(o => o.perguntaid == perguntaId);
            _context.opcoespergunta.RemoveRange(antigas);
            await _context.SaveChangesAsync();

            // insere novas
            foreach (var (texto, correta) in novas)
            {
                var op = new opcoespergunta
                {
                    perguntaid = perguntaId,
                    texto = (texto ?? string.Empty).Trim(),
                    correta = correta
                };
                await _repoOpcoes.IncluirAsync(op);
            }
        }

        /// <summary>
        /// Define a opção correta (objetiva). Passar null para limpar gabarito.
        /// </summary>
        public async Task AtualizarGabaritoObjetivaAsync(int perguntaId, int? corretaIndex)
        {
            var pergunta = await _repoPerguntas.SelecionarChaveAsync(perguntaId)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");
            if (pergunta.tipoperguntaid != TipoObjetiva)
                throw new InvalidOperationException("Pergunta não é do tipo objetiva.");

            var opcoes = await _context.opcoespergunta
                .Where(o => o.perguntaid == perguntaId)
                .OrderBy(o => o.opcaoid)
                .ToListAsync();

            for (int i = 0; i < opcoes.Count; i++)
                opcoes[i].correta = (corretaIndex.HasValue && i == corretaIndex.Value);

            pergunta.temgabarito = corretaIndex.HasValue;

            await _repoPerguntas.AlterarAsync(pergunta);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Define múltiplas corretas (múltipla escolha). Lista vazia para limpar gabarito.
        /// </summary>
        public async Task AtualizarGabaritoMultiplaAsync(int perguntaId, IEnumerable<int> corretasIdx)
        {
            var pergunta = await _repoPerguntas.SelecionarChaveAsync(perguntaId)
                           ?? throw new KeyNotFoundException("Pergunta não encontrada.");
            if (pergunta.tipoperguntaid != TipoMultipla)
                throw new InvalidOperationException("Pergunta não é do tipo múltipla.");

            var set = new HashSet<int>((corretasIdx ?? Enumerable.Empty<int>()).Distinct());

            var opcoes = await _context.opcoespergunta
                .Where(o => o.perguntaid == perguntaId)
                .OrderBy(o => o.opcaoid)
                .ToListAsync();

            for (int i = 0; i < opcoes.Count; i++)
                opcoes[i].correta = set.Contains(i);

            pergunta.temgabarito = set.Count > 0;

            await _repoPerguntas.AlterarAsync(pergunta);
            await _context.SaveChangesAsync();
        }

        // ===== Montagem em massa a partir da PesquisaVM (opcional, usado no ServicePesquisas) =====
        public async Task<List<perguntas>> CriarPerguntasAPartirDaVMAsync(int pesquisaId, PesquisaVM vm)
        {
            var criadas = new List<perguntas>();

            // Discursivas
            foreach (var d in vm.PerguntasDiscursivas ?? Enumerable.Empty<PerguntaDiscursivaDto>())
                criadas.Add(await CadastrarDiscursivaAsync(pesquisaId, d));

            // Objetivas
            foreach (var o in vm.PerguntasObjetivas ?? Enumerable.Empty<PerguntaObjetivaDto>())
                criadas.Add(await CadastrarObjetivaAsync(pesquisaId, o));

            // Múltiplas
            foreach (var m in vm.PerguntasMultiplaEscolha ?? Enumerable.Empty<PerguntaMultiplaEscolhaDto>())
                criadas.Add(await CadastrarMultiplaAsync(pesquisaId, m));

            return criadas;
        }
    }
}
