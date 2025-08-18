using FASTSURVEY.Models.DTO;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FASTSURVEY.Services
{
    public class ServiceRespostas
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryRespostas _repoRespostas;

        // IDs canônicos de tipo de pergunta (ajuste se a sua base for diferente)
        private const int TipoDiscursiva = 1;
        private const int TipoObjetiva = 2;
        private const int TipoMultipla = 3;

        public ServiceRespostas(FastSurveyContext context, RepositoryRespostas repositoryRespostas)
        {
            _context = context;
            _repoRespostas = repositoryRespostas;
        }

        // ===== CRUD “básico” (mantidos) =====
        public async Task<List<respostas>> ListarTodasRespostasAsync()
            => await _repoRespostas.SelecionarTodosAsync();

        public async Task<respostas> ObterRespostaPorIdAsync(int id)
            => await _repoRespostas.SelecionarChaveAsync(id);

        public async Task<respostas> CadastrarRespostaAsync(respostas resposta)
        {
            if (resposta == null)
                throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");

            return await _repoRespostas.IncluirAsync(resposta);
        }

        public async Task<respostas> AtualizarRespostaAsync(respostas resposta)
        {
            if (resposta == null)
                throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");

            var existente = await _repoRespostas.SelecionarChaveAsync(resposta.respostaid);
            if (existente == null)
                throw new KeyNotFoundException($"Resposta com ID {resposta.respostaid} não encontrada.");

            return await _repoRespostas.AlterarAsync(resposta);
        }

        public async Task<bool> ExcluirRespostaAsync(int id)
        {
            var resposta = await _repoRespostas.SelecionarChaveAsync(id);
            if (resposta == null)
                throw new KeyNotFoundException($"Resposta com ID {id} não encontrada.");

            await _repoRespostas.ExcluirAsync(resposta);
            return true;
        }

        // ======= NOVO: gravação em lote a partir do RespostaVM =======

        /// <summary>
        /// Recebe um RespostaVM com as respostas do usuário para uma pesquisa
        /// e grava tudo de forma consistente (respostas + vínculos com opções).
        /// </summary>
        public async Task GravarLoteAsync(RespostaVM vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.PesquisaId <= 0) throw new ArgumentException("PesquisaId inválido.");
            if (vm.Respostas == null || vm.Respostas.Count == 0) return;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Carrega perguntas da pesquisa (id + tipo)
                var perguntas = await _context.perguntas
                    .AsNoTracking()
                    .Where(p => p.pesquisaid == vm.PesquisaId)
                    .Select(p => new { p.perguntaid, p.tipoperguntaid })
                    .ToDictionaryAsync(p => p.perguntaid, p => p.tipoperguntaid);

                foreach (var item in vm.Respostas)
                {
                    if (string.IsNullOrWhiteSpace(item.Pergunta))
                        continue;

                    if (!int.TryParse(item.Pergunta, out var perguntaId))
                        throw new ArgumentException($"Pergunta '{item.Pergunta}' não é um ID válido.");

                    if (!perguntas.TryGetValue(perguntaId, out var tipo))
                        throw new KeyNotFoundException($"Pergunta {perguntaId} não pertence à pesquisa {vm.PesquisaId}.");

                    switch (tipo)
                    {
                        case TipoDiscursiva:
                            await GravarDiscursivaAsync(perguntaId, item.Resposta ?? string.Empty);
                            break;

                        case TipoObjetiva:
                            // Espera 1 única opção (ex.: "5" ou "[5]")
                            var unica = ParseOpcoes(item.Resposta).FirstOrDefault();
                            if (unica == 0)
                                throw new ArgumentException($"Resposta inválida para pergunta objetiva {perguntaId}.");
                            await GravarObjetivaAsync(perguntaId, unica);
                            break;

                        case TipoMultipla:
                            // Pode vir "1,2,3" ou "[1,2,3]"
                            var selecionadas = ParseOpcoes(item.Resposta).ToList();
                            await GravarMultiplaAsync(perguntaId, selecionadas);
                            break;

                        default:
                            throw new InvalidOperationException($"Tipo de pergunta desconhecido: {tipo}");
                    }
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ----------------- helpers internos -----------------

        private async Task GravarDiscursivaAsync(int perguntaId, string texto)
        {
            // respostas.texto é NOT NULL → se vier null, enviamos string vazia
            var r = new respostas
            {
                perguntaid = perguntaId,
                texto = texto ?? string.Empty
            };
            await _repoRespostas.IncluirAsync(r);
        }

        private async Task GravarObjetivaAsync(int perguntaId, int opcaoId)
        {
            // Cria a resposta “vazia” (texto obrigatório, mas não usamos para objetiva)
            var r = new respostas
            {
                perguntaid = perguntaId,
                texto = string.Empty
            };
            await _repoRespostas.IncluirAsync(r);

            // Vincula a opção escolhida em respostas_opcoes
            var vinc = new respostas_opcoes
            {
                respostaid = r.respostaid,
                opcaoid = opcaoId
            };
            _context.respostas_opcoes.Add(vinc);
            await _context.SaveChangesAsync();
        }

        private async Task GravarMultiplaAsync(int perguntaId, IEnumerable<int> opcoesIds)
        {
            // Cria a resposta “vazia”
            var r = new respostas
            {
                perguntaid = perguntaId,
                texto = string.Empty
            };
            await _repoRespostas.IncluirAsync(r);

            // Vários vínculos
            var lista = (opcoesIds ?? Enumerable.Empty<int>())
                .Distinct()
                .Select(id => new respostas_opcoes
                {
                    respostaid = r.respostaid,
                    opcaoid = id
                })
                .ToList();

            if (lista.Count > 0)
                _context.respostas_opcoes.AddRange(lista);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Converte “1,2,3” ou “[1,2,3]” ou “1” em IEnumerable&lt;int&gt;.
        /// </summary>
        private static IEnumerable<int> ParseOpcoes(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Enumerable.Empty<int>();

            raw = raw.Trim();

            // JSON array?
            if (raw.StartsWith("[") && raw.EndsWith("]"))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<int>>(raw);
                    return arr ?? Enumerable.Empty<int>();
                }
                catch { /* cai para parse CSV abaixo */ }
            }

            // CSV/único
            var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var nums = new List<int>();
            foreach (var p in parts)
                if (int.TryParse(p, out var n)) nums.Add(n);

            return nums;
        }
    }
}
