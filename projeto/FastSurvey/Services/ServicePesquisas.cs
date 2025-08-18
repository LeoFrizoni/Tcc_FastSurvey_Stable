using FASTSURVEY.Models.DTO;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Services
{
    public class ServicePesquisas
    {
        private readonly FastSurveyContext _context;

        private readonly RepositoryAnexos _repositoryAnexos;
        private readonly RepositoryPesquisas _repoPesquisas;
        private readonly RepositoryPerguntas _repoPerguntas;
        private readonly RepositoryTipoPergunta _repoTipoPergunta;
        private readonly RepositoryTipoPesquisa _repoTipoPesquisa;
        private readonly RepositoryRespostas _repoRespostas;

        // Serviço especializado para perguntas (criação a partir dos DTOs)
        private readonly ServicePerguntas _servicePerguntas;

        public ServicePesquisas(FastSurveyContext context)
        {
            _context = context;

            _repositoryAnexos = new RepositoryAnexos(_context, true);
            _repoPesquisas = new RepositoryPesquisas(_context, true);
            _repoPerguntas = new RepositoryPerguntas(_context, true);
            _repoTipoPergunta = new RepositoryTipoPergunta(_context, true);
            _repoTipoPesquisa = new RepositoryTipoPesquisa(_context, true);
            _repoRespostas = new RepositoryRespostas(_context, true);

            _servicePerguntas = new ServicePerguntas(_context);
        }

        public async Task<List<pesquisas>> ListarTodasPesquisasAsync()
        {
            return await _repoPesquisas.SelecionarTodosAsync();
        }

        public async Task<object> BuscarPesquisaPorIdAsync(int id)
        {
            try
            {
                var resultado = await _context.pesquisas
                    .Include(p => p.perguntas)
                        .ThenInclude(p => p.opcoespergunta)
                    .Include(p => p.login)
                    .Where(p => p.pesquisaid == id)
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
                                tipo = pergunta.tipoperguntaid == 1 ? "discursiva" :
                                       pergunta.tipoperguntaid == 2 ? "objetiva" :
                                       pergunta.tipoperguntaid == 3 ? "multipla" : "desconhecida",
                                tipoPerguntaId = pergunta.tipoperguntaid,
                                temGabarito = pergunta.temgabarito,
                                permitirMultiplaSelecao = pergunta.permitemultiplaselecao,
                                opcoes = pergunta.opcoespergunta
                                    .OrderBy(o => o.opcaoid)
                                    .Select(o => new
                                    {
                                        opcaoid = o.opcaoid,
                                        texto = o.texto,
                                        correta = o.correta
                                    })
                                    .ToList()
                            }).ToList(),
                        templateJson = p.TemplateJson
                    })
                    .FirstOrDefaultAsync();

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

        /// <summary>
        /// Cria a Pesquisa e, em seguida, cria TODAS as perguntas (discursivas/objetivas/múltiplas)
        /// a partir da PesquisaVM. Tudo em transação.
        /// </summary>
        public async Task<pesquisas> CadastrarPesquisaAsync(FASTSURVEY.Models.PesquisaVM vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (string.IsNullOrWhiteSpace(vm.Titulo)) throw new ArgumentException("Título da pesquisa é obrigatório.");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var pesquisa = new pesquisas
                {
                    tipopesquisaid = vm.TipoPesquisaId,
                    titulo = vm.Titulo,
                    descricao = vm.Descricao ?? string.Empty,
                    loginid = vm.LoginId,
                    TemplateJson = vm.TemplateJson
                };

                // Primeiro inclui a PESQUISA para obter o ID
                await _repoPesquisas.IncluirAsync(pesquisa);

                // Agora cria as PERGUNTAS vinculadas a esta pesquisa
                await _servicePerguntas.CriarPerguntasAPartirDaVMAsync(pesquisa.pesquisaid, vm);

                await tx.CommitAsync();
                return pesquisa;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new Exception("Erro ao cadastrar pesquisa: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza dados básicos e TemplateJson. (Atualização granular de perguntas pode ser feita via ServicePerguntas)
        /// </summary>
        public async Task<pesquisas> AtualizarPesquisaAsync(FASTSURVEY.Models.PesquisaVM vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.CodigoPesquisa <= 0) throw new ArgumentException("Código da pesquisa inválido.");

            var pesquisa = await _repoPesquisas.SelecionarChaveAsync(vm.CodigoPesquisa);
            if (pesquisa == null)
                throw new Exception("Pesquisa não encontrada.");

            pesquisa.titulo = vm.Titulo ?? pesquisa.titulo;
            pesquisa.descricao = vm.Descricao ?? pesquisa.descricao;
            pesquisa.tipopesquisaid = vm.TipoPesquisaId > 0 ? vm.TipoPesquisaId : pesquisa.tipopesquisaid;
            pesquisa.TemplateJson = vm.TemplateJson ?? pesquisa.TemplateJson;

            return await _repoPesquisas.AlterarAsync(pesquisa);
        }

        public async Task ExcluirPesquisaAsync(int id)
        {
            var pesquisa = await _repoPesquisas.SelecionarChaveAsync(id);
            if (pesquisa != null)
            {
                await _repoPesquisas.ExcluirAsync(pesquisa);
            }
        }
    }
}
