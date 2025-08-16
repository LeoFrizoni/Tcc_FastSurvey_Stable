using FASTSURVEY.Models;
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
        private FastSurveyContext _context;
        private RepositoryAnexos _repositoryAnexos { get; set; }
        private RepositoryPesquisas _RepositoryPesquisas { get; set; }
        private RepositoryPerguntas _RepositoryPerguntas { get; set; }
        private RepositoryTipoPergunta _RepositoryTipoPergunta { get; set; }
        private RepositoryTipoPesquisa _RepositoryTipoPesquisa { get; set; }
        private RepositoryRespostas _RepositoryRespostas { get; set; }

        public ServicePesquisas(FastSurveyContext context)
        {
            _context = context;
            _repositoryAnexos = new RepositoryAnexos(_context, true);
            _RepositoryPesquisas = new RepositoryPesquisas(_context, true);
            _RepositoryPerguntas = new RepositoryPerguntas(_context, true);
            _RepositoryTipoPergunta = new RepositoryTipoPergunta(_context, true);
            _RepositoryTipoPesquisa = new RepositoryTipoPesquisa(_context, true);
            _RepositoryRespostas = new RepositoryRespostas(_context, true);
        }

        public async Task<List<pesquisas>> ListarTodasPesquisasAsync()
        {
            return await _RepositoryPesquisas.SelecionarTodosAsync();
        }

        public async Task<object> BuscarPesquisaPorIdAsync(int id)
        {
            try
            {
                var resultado = await _context.pesquisas
                    .Include(p => p.perguntas)
                        .ThenInclude(p => p.opcoespergunta)
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
                        perguntas = p.perguntas.Select(pergunta => new
                        {
                            perguntaid = pergunta.perguntaid,
                            titulo = pergunta.texto,
                            tipo = pergunta.tipoperguntaid == 1 ? "discursiva" :
                                   pergunta.tipoperguntaid == 2 ? "objetiva" :
                                   pergunta.tipoperguntaid == 3 ? "multipla" : "desconhecida",
                            tipoPerguntaId = pergunta.tipoperguntaid,
                            opcoes = pergunta.opcoespergunta.Select(o => new
                            {
                                opcaoid = o.opcaoid,
                                texto = o.texto
                            }).ToList()
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

        public async Task<pesquisas> CadastrarPesquisaAsync(PesquisaVM pesquisaVM)
        {
            try
            {
                var listaPerguntas = new List<perguntas>();

                // Mapeamento das perguntas discursivas
                foreach (var item in pesquisaVM.PerguntasDiscursivas ?? new List<PerguntaDiscursiva>())
                {
                    var pergunta = new perguntas()
                    {
                        texto = item.Titulo,
                        tipoperguntaid = 1
                        // Não adiciona respostas na criação da pesquisa
                    };
                    listaPerguntas.Add(pergunta);
                }

                // Mapeamento das perguntas objetivas
                foreach (var item in pesquisaVM.PerguntasObjetivas ?? new List<PerguntaObjetiva>())
                {
                    var pergunta = new perguntas()
                    {
                        texto = item.Titulo,
                        tipoperguntaid = 2,
                        opcoespergunta = item.Opcoes.Select(opcao => new opcoespergunta
                        {
                            texto = opcao.Opcao
                        }).ToList()
                    };
                    listaPerguntas.Add(pergunta);
                }

                // Mapeamento das perguntas de múltipla escolha
                foreach (var item in pesquisaVM.PerguntasMultiplaEscolha ?? new List<PerguntaMultiplaEscolha>())
                {
                    var pergunta = new perguntas()
                    {
                        texto = item.Titulo,
                        tipoperguntaid = 3,
                        opcoespergunta = item.Opcoes.Select(opcao => new opcoespergunta
                        {
                            texto = opcao.Opcao
                        }).ToList()
                    };
                    listaPerguntas.Add(pergunta);
                }

                var pesquisa = new pesquisas
                {
                    tipopesquisaid = pesquisaVM.TipoPesquisaId,
                    titulo = pesquisaVM.Titulo,
                    descricao = pesquisaVM.Descricao,
                    loginid = pesquisaVM.LoginId,
                    perguntas = listaPerguntas,
                    TemplateJson = pesquisaVM.TemplateJson // ✅ Campo novo adicionado
                };

                return await _RepositoryPesquisas.IncluirAsync(pesquisa);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao cadastrar pesquisa: " + ex.Message);
            }
        }

        public async Task<pesquisas> AtualizarPesquisaAsync(PesquisaVM pesquisaVM)
        {
            var pesquisaExistente = await _RepositoryPesquisas.SelecionarChaveAsync(pesquisaVM.CodigoPesquisa);

            if (pesquisaExistente == null)
                throw new Exception("Pesquisa não encontrada.");

            pesquisaExistente.titulo = pesquisaVM.Titulo;
            pesquisaExistente.tipopesquisaid = pesquisaVM.TipoPesquisaId;
            pesquisaExistente.TemplateJson = pesquisaVM.TemplateJson; // ✅ Atualiza o template também

            // Atualização de perguntas pode ser adicionada aqui se necessário

            return await _RepositoryPesquisas.AlterarAsync(pesquisaExistente);
        }

        public async Task ExcluirPesquisaAsync(int id)
        {
            var pesquisa = await _RepositoryPesquisas.SelecionarChaveAsync(id);
            if (pesquisa != null)
            {
                await _RepositoryPesquisas.ExcluirAsync(pesquisa);
            }
        }
    }
}
