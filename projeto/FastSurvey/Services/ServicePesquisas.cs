// FASTSURVEY/Services/ServicePesquisas.cs
using FASTSURVEY.Models;            // PesquisaVM
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
    public class ServicePesquisas
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryAnexos _repoAnexos;
        private readonly RepositoryPesquisas _repoPesquisas;
        private readonly RepositoryPerguntas _repoPerguntas;
        private readonly RepositoryTipoPergunta _repoTipoPergunta;
        private readonly RepositoryTipoPesquisa _repoTipoPesquisa;
        private readonly RepositoryRespostas _repoRespostas;
        private readonly ServicePerguntas _servicePerguntas;

        public ServicePesquisas(FastSurveyContext context)
        {
            _context = context;
            _repoAnexos = new RepositoryAnexos(_context, true);
            _repoPesquisas = new RepositoryPesquisas(_context, true);
            _repoPerguntas = new RepositoryPerguntas(_context, true);
            _repoTipoPergunta = new RepositoryTipoPergunta(_context, true);
            _repoTipoPesquisa = new RepositoryTipoPesquisa(_context, true);
            _repoRespostas = new RepositoryRespostas(_context, true);
            _servicePerguntas = new ServicePerguntas(_context);
        }

        /// <summary>
        /// Move uma pesquisa para outra pasta.
        /// </summary>
        public async Task<bool> MoverPesquisaParaPastaAsync(int pesquisaId, int novaPastaId, CancellationToken ct = default)
        {
            if (pesquisaId <= 0 || novaPastaId <= 0)
                throw new ArgumentException("ID da pesquisa e da pasta devem ser válidos.");

            var pesquisa = await _context.pesquisas.FirstOrDefaultAsync(p => p.pesquisaid == pesquisaId, ct);
            if (pesquisa == null)
                return false;

            var pastaExiste = await _context.pastas.AnyAsync(p => p.pastaid == novaPastaId, ct);
            if (!pastaExiste)
                return false;

            pesquisa.pastaid = novaPastaId;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // =========================
        // Listagem / Busca
        // =========================

        public async Task<List<pesquisas>> ListarTodasPesquisasAsync(CancellationToken ct = default)
        {
            return await _context.pesquisas
                .AsNoTracking()
                .OrderBy(p => p.pesquisaid)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Lista pesquisas pertencentes ao usuário informado.
        /// Ajuste o campo de vínculo se não for "loginid".
        /// </summary>
        public async Task<List<pesquisas>> ListarPorUsuarioAsync(int usuarioId, CancellationToken ct = default)
        {
            if (usuarioId <= 0) return new List<pesquisas>();

            return await _context.pesquisas
                .AsNoTracking()
                .Where(p => p.loginid == usuarioId)    // troque se o campo for outro
                .OrderBy(p => p.pesquisaid)
                .ToListAsync(ct);
        }

        public async Task<object> BuscarPesquisaPorIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _context.pesquisas
                    .AsNoTracking()
                    .Where(p => p.pesquisaid == id)
                    .Include(p => p.perguntas)
                        .ThenInclude(pg => pg.opcoespergunta)
                    .Include(p => p.login)
                    .Include(p => p.tipopesquisa)
                    .Select(p => new
                    {
                        pesquisaid = p.pesquisaid,
                        titulo = p.titulo,
                        descricao = p.descricao,
                        tipoPesquisa = new
                        {
                            descricao = p.tipopesquisa != null ? p.tipopesquisa.tipopesquisa1 : "Indefinido"
                        },
                        autor = p.login != null ? p.login.usuario : "",
                        dataCriacao = p.login != null ? p.login.dataregistro : (DateTime?)null,
                        perguntas = p.perguntas
                            .OrderBy(pg => pg.perguntaid)
                            .Select(pergunta => new
                            {
                                perguntaid = pergunta.perguntaid,
                                titulo = pergunta.texto,
                                tipo = pergunta.tipoperguntaid == 1 ? "discursiva"
                                     : pergunta.tipoperguntaid == 2 ? "objetiva"
                                     : pergunta.tipoperguntaid == 3 ? "multipla" : "desconhecida",
                                tipoPerguntaId = pergunta.tipoperguntaid,
                                temGabarito = pergunta.temgabarito,
                                permitirMultiplaSelecao = pergunta.permitemultiplaselecao,
                                opcoes = pergunta.opcoespergunta
                                    .OrderBy(o => o.opcaoid)
                                    .Select(o => new { o.opcaoid, o.texto, o.correta })
                                    .ToList()
                            }).ToList(),
                        templateJson = p.TemplateJson
                    })
                    .FirstOrDefaultAsync(ct);

                if (resultado == null)
                    return new { mensagem = "Pesquisa não encontrada." };

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar pesquisa por ID: {ex.Message}");
                throw;
            }
        }

        // =========================
        // Cadastro / Atualização
        // =========================

        /// <summary>
        /// Cria a pesquisa e, em seguida, todas as perguntas (discursivas/objetivas/múltiplas)
        /// a partir da PesquisaVM. Tudo em transação.
        /// </summary>
        public async Task<pesquisas> CadastrarPesquisaAsync(PesquisaVM vm, CancellationToken ct = default)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (string.IsNullOrWhiteSpace(vm.Titulo)) throw new ArgumentException("Título da pesquisa é obrigatório.");
            if (vm.TipoPesquisaId <= 0) throw new ArgumentException("Tipo de pesquisa inválido.");

            // valida se o tipo existe (evita FK inválida)
            var tipoExiste = await _context.tipopesquisa
                .AsNoTracking()
                .AnyAsync(t => t.tipopesquisaid == vm.TipoPesquisaId, ct);
            if (!tipoExiste)
                throw new ArgumentException("Tipo de pesquisa não encontrado.");

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var pesquisa = new pesquisas
                {
                    tipopesquisaid = vm.TipoPesquisaId,
                    titulo = (vm.Titulo ?? string.Empty).Trim(),
                    descricao = (vm.Descricao ?? string.Empty).Trim(),
                    loginid = vm.LoginId,
                    TemplateJson = vm.TemplateJson,
                    datacriacao = vm.DataCriacao ?? DateTime.Now
                };

                await _context.pesquisas.AddAsync(pesquisa, ct);
                await _context.SaveChangesAsync(ct); // precisa do ID

                // cria as perguntas vinculadas a esta pesquisa
                await _servicePerguntas.CriarPerguntasAPartirDaVMAsync(pesquisa.pesquisaid, vm); // ServicePerguntas sem ct

                await tx.CommitAsync(ct);
                return pesquisa;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                throw new Exception("Erro ao cadastrar pesquisa: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza dados básicos e TemplateJson. (Perguntas/Opções: usar ServicePerguntas)
        /// </summary>
        public async Task<pesquisas> AtualizarPesquisaAsync(PesquisaVM vm, CancellationToken ct = default)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.CodigoPesquisa <= 0) throw new ArgumentException("Código da pesquisa inválido.");

            // Repositório não tem sobrecarga com ct
            var pesquisa = await _repoPesquisas.SelecionarChaveAsync(vm.CodigoPesquisa);
            if (pesquisa == null)
                throw new Exception("Pesquisa não encontrada.");

            if (!string.IsNullOrWhiteSpace(vm.Titulo))
                pesquisa.titulo = vm.Titulo.Trim();

            if (vm.Descricao != null)
                pesquisa.descricao = vm.Descricao.Trim();

            if (vm.TipoPesquisaId > 0 && vm.TipoPesquisaId != pesquisa.tipopesquisaid)
            {
                // (opcional) validar existência do novo tipo
                var tipoExiste = await _context.tipopesquisa
                    .AsNoTracking()
                    .AnyAsync(t => t.tipopesquisaid == vm.TipoPesquisaId, ct);
                if (!tipoExiste)
                    throw new ArgumentException("Tipo de pesquisa não encontrado.");

                pesquisa.tipopesquisaid = vm.TipoPesquisaId;
            }

            if (vm.TemplateJson != null)
                pesquisa.TemplateJson = vm.TemplateJson;

            // Repositório sem ct
            return await _repoPesquisas.AlterarAsync(pesquisa);
        }

        // =========================
        // Exclusão (com limpeza de vínculos)
        // =========================

        /// <summary>
        /// Exclui a pesquisa e TODOS os seus vínculos (perguntas, opções, respostas, anexos).
        /// Executa de forma transacional. Usa ExecuteDeleteAsync quando disponível.
        /// </summary>
        public async Task ExcluirPesquisaAsync(int id, CancellationToken ct = default)
        {
            // Repositório sem ct
            var pesquisa = await _repoPesquisas.SelecionarChaveAsync(id);
            if (pesquisa == null) return;

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // Coleta IDs das perguntas para limpar dependências
                var perguntasIds = await _context.perguntas
                    .Where(p => p.pesquisaid == id)
                    .Select(p => p.perguntaid)
                    .ToListAsync(ct);

                if (perguntasIds.Count > 0)
                {
                    // Respostas associadas às perguntas desta pesquisa
                    var respostasQ = _context.respostas.Where(r => perguntasIds.Contains(r.perguntaid));
#if NET7_0_OR_GREATER
                    await respostasQ.ExecuteDeleteAsync(ct);
#else
                    var respostas = await respostasQ.ToListAsync(ct);
                    if (respostas.Count > 0) _context.respostas.RemoveRange(respostas);
                    await _context.SaveChangesAsync(ct);
#endif

                    // Opções das perguntas
                    var opcoesQ = _context.opcoespergunta.Where(o => perguntasIds.Contains(o.perguntaid));
#if NET7_0_OR_GREATER
                    await opcoesQ.ExecuteDeleteAsync(ct);
#else
                    var opcoes = await opcoesQ.ToListAsync(ct);
                    if (opcoes.Count > 0) _context.opcoespergunta.RemoveRange(opcoes);
                    await _context.SaveChangesAsync(ct);
#endif

                    // Perguntas
                    var perguntasQ = _context.perguntas.Where(p => p.pesquisaid == id);
#if NET7_0_OR_GREATER
                    await perguntasQ.ExecuteDeleteAsync(ct);
#else
                    var perguntas = await perguntasQ.ToListAsync(ct);
                    if (perguntas.Count > 0) _context.perguntas.RemoveRange(perguntas);
                    await _context.SaveChangesAsync(ct);
#endif
                }

                // Anexos da pesquisa (se houver)
                var anexosQ = _context.anexos.Where(a => a.pesquisaid == id);
#if NET7_0_OR_GREATER
                await anexosQ.ExecuteDeleteAsync(ct);
#else
                var anexos = await anexosQ.ToListAsync(ct);
                if (anexos.Count > 0) _context.anexos.RemoveRange(anexos);
                await _context.SaveChangesAsync(ct);
#endif

                // Finalmente, a pesquisa
#if NET7_0_OR_GREATER
                await _context.pesquisas.Where(p => p.pesquisaid == id).ExecuteDeleteAsync(ct);
#else
                _context.pesquisas.Remove(pesquisa);
                await _context.SaveChangesAsync(ct);
#endif

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
